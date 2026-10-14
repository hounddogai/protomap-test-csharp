namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual pay band template.</summary>
public sealed record ManualPayBandTemplateView(string CodeText, string NameText, string StartsAtText, string SequenceText, string Quantity, string MaximumText, string StatusLabel)
{
    public static ManualPayBandTemplateView ToManualPayBandTemplateView(ManualPayBandTemplate item)
    {
        return new ManualPayBandTemplateView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPayBandTemplateView> ToManualPayBandTemplateViews(List<ManualPayBandTemplate> items)
    {
        var views = new List<ManualPayBandTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToManualPayBandTemplateView(item));
        }
        return views;
    }

    public static double SumManualPayBandTemplateView(ManualPayBandTemplateView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedSequence + (double)parsedQuantity;
    }
}
