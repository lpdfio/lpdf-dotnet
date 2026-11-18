namespace Lpdf.Engine;

/// <summary>
/// How the values of each type are written on the page, for <see cref="LCrossIndustryInvoice"/>: each function takes
/// the value in its normal form and the full name of its entry in the map, without the prefix or an item number
/// (<c>seller.country</c>, <c>lines.unit</c>), and returns the text to bind, never <see langword="null"/>: return the
/// value as it came to keep it. A type without a function is bound as the invoice has it.
/// </summary>
public sealed class InvoiceFormat
{
    /// <summary>A date, as <c>2026-10-31</c>, or <c>2026-10</c> and <c>2026-W44</c> for a month and a week.</summary>
    public Func<string, string, string>? Date { get; init; }

    /// <summary>An amount, as the invoice writes it: <c>1250.10</c>.</summary>
    public Func<string, string, string>? Amount { get; init; }

    /// <summary>A code, as the invoice writes it: <c>H87</c>, <c>S</c>, <c>381</c>. Lpdf carries no words for codes.</summary>
    public Func<string, string, string>? Code { get; init; }
}
