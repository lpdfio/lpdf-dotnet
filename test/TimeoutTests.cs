using System.Diagnostics;
using System.Security.Cryptography;
using Lpdf.Engine;
using Xunit;

namespace Lpdf.Tests;

/// <summary>
/// The timeout, an epoch deadline on each render's store. A render that runs long is a small WebAssembly module of the
/// test's own, run in place of the engine: the real engine renders the largest fixture in well under a second.
/// </summary>
public class TimeoutTests
{
    private const string SpinForever = """(module (func (export "_start") (loop $spin (br $spin))))""";

    // Spins on the monotonic clock for two seconds, then writes a response that holds the bytes "%PDF-".
    private const string SpinTwoSeconds = """
        (module
          (import "wasi_snapshot_preview1" "clock_time_get" (func $clock (param i32 i64 i32) (result i32)))
          (import "wasi_snapshot_preview1" "fd_write" (func $write (param i32 i32 i32 i32) (result i32)))
          (memory (export "memory") 1)
          (data (i32.const 32) "{\"pdf\": \"JVBERi0=\"}")
          (func (export "_start")
            (local $until i64)
            (drop (call $clock (i32.const 1) (i64.const 1) (i32.const 0)))
            (local.set $until (i64.add (i64.load (i32.const 0)) (i64.const 2000000000)))
            (loop $spin
              (drop (call $clock (i32.const 1) (i64.const 1) (i32.const 0)))
              (br_if $spin (i64.lt_u (i64.load (i32.const 0)) (local.get $until))))
            (i32.store (i32.const 16) (i32.const 32))
            (i32.store (i32.const 20) (i32.const 19))
            (drop (call $write (i32.const 1) (i32.const 16) (i32.const 1) (i32.const 24)))))
        """;

    private const string Minimal =
        "<lpdf version=\"1\"><document><section><layout><text>x</text></layout></section></document></lpdf>";

    [Fact]
    public void A_render_past_its_timeout_throws_naming_the_limit()
    {
        using var engine = StandIn(SpinTwoSeconds, new EngineOptions { Timeout = 1 });

        var (elapsed, error) = TimedRender(engine);

        Assert.Equal("The render timed out after 1 second.", error?.Message);
        Assert.InRange(elapsed.TotalSeconds, 1, 1.5);
    }

    [Fact]
    public void A_render_with_a_timeout_of_0_runs_with_no_limit()
    {
        using var engine = StandIn(SpinTwoSeconds, new EngineOptions { Timeout = 0 });

        var stopwatch = Stopwatch.StartNew();
        var pdf = engine.Render(Minimal);

        Assert.Equal("%PDF-"u8.ToArray(), pdf);
        Assert.True(stopwatch.Elapsed.TotalSeconds >= 2, $"rendered in {stopwatch.Elapsed.TotalSeconds} s");
    }

    [Fact]
    public void A_render_stops_after_5_seconds_when_the_timeout_is_left_out()
    {
        using var engine = StandIn(SpinForever, new EngineOptions());

        var (elapsed, error) = TimedRender(engine);

        Assert.Equal("The render timed out after 5 seconds.", error?.Message);
        Assert.InRange(elapsed.TotalSeconds, 5, 5.5);
    }

    [Fact]
    public void The_timeout_is_5_seconds_when_left_out()
    {
        Assert.Equal(5, new EngineOptions().Timeout);
    }

    [Fact]
    public void A_negative_timeout_is_refused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EngineOptions { Timeout = -1 });
    }

    /// <summary>
    /// The Wasmtime engine and its epoch are the process's, shared by every PdfEngine: a render stopped at its
    /// deadline stops nothing else, and leaves the engine rendering right.
    /// </summary>
    [Fact]
    public void A_render_past_its_timeout_leaves_the_others_and_the_next_render_right()
    {
        if (!SnapshotHelper.HAS_FIXTURES)
            return; // Fixture files not available outside the monorepo.

        using var spinning = StandIn(SpinForever, new EngineOptions { Timeout = 1 });
        using var barrier  = new Barrier(2);
        Exception? error   = null;
        string?    hash    = null;
        long stopped = 0, rendered = 0;

        var timeOut = new Thread(() =>
        {
            barrier.SignalAndWait();
            (_, error) = TimedRender(spinning);
            stopped = Stopwatch.GetTimestamp();
        });
        var renderMeanwhile = new Thread(() =>
        {
            barrier.SignalAndWait();
            hash = RenderFixture("bench_m", new EngineOptions { Timeout = 1 });
            rendered = Stopwatch.GetTimestamp();
        });
        timeOut.Start();
        renderMeanwhile.Start();
        timeOut.Join();
        renderMeanwhile.Join();

        Assert.Equal("The render timed out after 1 second.", error?.Message);
        Assert.Equal(Snapshot("bench_m"), hash);
        Assert.True(rendered < stopped, "the render meanwhile ended after the one that timed out");
        Assert.Equal(Snapshot("bench_m"), RenderFixture("bench_m", new EngineOptions { Timeout = 1 }));
    }

    [Fact]
    public void A_fixture_renders_with_a_timeout_of_0()
    {
        if (!SnapshotHelper.HAS_FIXTURES)
            return; // Fixture files not available outside the monorepo.

        Assert.Equal(Snapshot("bench_l"), RenderFixture("bench_l", new EngineOptions { Timeout = 0 }));
    }

    private static PdfEngine StandIn(string wat, EngineOptions options)
        => new(options, WasmRunner.ForModuleText(wat, options.Timeout));

    private static (TimeSpan Elapsed, EngineException? Error) TimedRender(PdfEngine engine)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            engine.Render(Minimal);
            return (stopwatch.Elapsed, null);
        }
        catch (EngineException e)
        {
            return (stopwatch.Elapsed, e);
        }
    }

    /// <summary>The SHA-256 of a fixture rendered as the snapshot tests render it.</summary>
    private static string RenderFixture(string name, EngineOptions options)
    {
        var xml = File.ReadAllText(Path.Combine(SnapshotHelper.Fixtures, $"{name}.xml"));
        using var engine = L.Engine(options).SetLicenseKey("test-key");
        var pdf = engine.Render(xml, new RenderOptions { SkipEncryption = true });
        return Convert.ToHexString(SHA256.HashData(pdf)).ToLowerInvariant();
    }

    private static string Snapshot(string name)
        => File.ReadAllText(Path.Combine(SnapshotHelper.Snapshots, $"{name}.pdf.sha256")).Trim();
}
