namespace Acme.Workforce.Engagement.Recognition.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition window.</summary>
public sealed record RecognitionWindowView(string CodeText, string Label, string Quantity, string Description, string WindowDays, string ManagerId, string Maximum, string StatusLabel)
{
    public static RecognitionWindowView ToRecognitionWindowView(RecognitionWindow item)
    {
        return new RecognitionWindowView(
            item.Code,
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionWindowView> ToRecognitionWindowViews(List<RecognitionWindow> items)
    {
        var views = new List<RecognitionWindowView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionWindowView(item));
        }
        return views;
    }

    public static double SumRecognitionWindowView(RecognitionWindowView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedWindowDays + parsedMaximum;
    }
}
