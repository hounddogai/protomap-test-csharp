namespace Acme.Workforce.Compliance.Exception.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception profile.</summary>
public sealed record ExceptionProfileView(string Code, string NameText, string MarginText, string OwnerId, string Channel, string Description, string Grade, string Notes, string ApprovedText, string Retries, string StatusLabel)
{
    public static ExceptionProfileView ToExceptionProfileView(ExceptionProfile item)
    {
        return new ExceptionProfileView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Channel,
            Text.Truncate(item.Description, 40),
            item.Grade,
            item.Notes,
            $"{item.Approved}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionProfileView> ToExceptionProfileViews(List<ExceptionProfile> items)
    {
        var views = new List<ExceptionProfileView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionProfileView(item));
        }
        return views;
    }

    public static double SumExceptionProfileView(ExceptionProfileView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedRetries;
    }
}
