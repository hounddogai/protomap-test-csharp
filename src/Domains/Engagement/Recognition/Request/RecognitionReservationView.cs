namespace Acme.Workforce.Engagement.Recognition.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition reservation.</summary>
public sealed record RecognitionReservationView(string Code, string Title, string OwnerId, string Timezone, string LimitText, string Skill, string Location, string GraceDays, string Category, string Unit, string StatusLabel)
{
    public static RecognitionReservationView ToRecognitionReservationView(RecognitionReservation item)
    {
        return new RecognitionReservationView(
            Text.Truncate(item.Code, 40),
            item.Title,
            Text.Truncate(item.OwnerId, 64),
            item.Timezone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Location,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionReservationView> ToRecognitionReservationViews(List<RecognitionReservation> items)
    {
        var views = new List<RecognitionReservationView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionReservationView(item));
        }
        return views;
    }

    public static double SumRecognitionReservationView(RecognitionReservationView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedGraceDays;
    }
}
