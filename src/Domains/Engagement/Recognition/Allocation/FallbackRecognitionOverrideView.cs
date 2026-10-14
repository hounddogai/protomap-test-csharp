namespace Acme.Workforce.Engagement.Recognition.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback recognition override.</summary>
public sealed record FallbackRecognitionOverrideView(string Code, string Title, string Amount, string Region, string EmployeeId, string Unit, string Priority, string CreatedAt, string Phone, string Score, string Budget, string StatusLabel)
{
    public static FallbackRecognitionOverrideView ToFallbackRecognitionOverrideView(FallbackRecognitionOverride item)
    {
        return new FallbackRecognitionOverrideView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 64),
            Text.Truncate(item.EmployeeId, 120),
            item.Unit,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackRecognitionOverrideView> ToFallbackRecognitionOverrideViews(List<FallbackRecognitionOverride> items)
    {
        var views = new List<FallbackRecognitionOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackRecognitionOverrideView(item));
        }
        return views;
    }

    public static double SumFallbackRecognitionOverrideView(FallbackRecognitionOverrideView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedPriority + (double)parsedCreatedAt;
    }
}
