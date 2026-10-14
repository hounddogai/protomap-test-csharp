namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a retro pay segment.</summary>
public sealed record RetroPaySegmentView(string Code, string Title, string CeilingText, string Region, string LocationText, string ContactEmail, string Name, string TeamText, string StatusLabel)
{
    public static RetroPaySegmentView ToRetroPaySegmentView(RetroPaySegment item)
    {
        return new RetroPaySegmentView(
            item.Code,
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Location,
            item.ContactEmail,
            item.Name,
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPaySegmentView> ToRetroPaySegmentViews(List<RetroPaySegment> items)
    {
        var views = new List<RetroPaySegmentView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPaySegmentView(item));
        }
        return views;
    }

    public static double SumRetroPaySegmentView(RetroPaySegmentView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
