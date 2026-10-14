namespace Acme.Workforce.Engagement.Recognition.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition plan.</summary>
public sealed record RecognitionPlanView(string Code, string Title, string SalaryBandText, string Region, string Quantity, string StatusLabel)
{
    public static RecognitionPlanView ToRecognitionPlanView(RecognitionPlan item)
    {
        return new RecognitionPlanView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.SalaryBand, 120),
            item.Region,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionPlanView> ToRecognitionPlanViews(List<RecognitionPlan> items)
    {
        var views = new List<RecognitionPlanView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionPlanView(item));
        }
        return views;
    }

    public static double SumRecognitionPlanView(RecognitionPlanView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
