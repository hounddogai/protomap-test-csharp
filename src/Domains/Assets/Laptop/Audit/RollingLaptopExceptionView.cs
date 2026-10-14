namespace Acme.Workforce.Assets.Laptop.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling laptop exception.</summary>
public sealed record RollingLaptopExceptionView(string CodeText, string LabelText, string StartsAtText, string SequenceText, string Maximum, string Name, string ChannelText, string DurationMinutes, string Title, string DepartmentText, string UpdatedAtText, string StatusLabel)
{
    public static RollingLaptopExceptionView ToRollingLaptopExceptionView(RollingLaptopException item)
    {
        return new RollingLaptopExceptionView(
            item.Code,
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 40),
            item.Channel,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Department,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingLaptopExceptionView> ToRollingLaptopExceptionViews(List<RollingLaptopException> items)
    {
        var views = new List<RollingLaptopExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToRollingLaptopExceptionView(item));
        }
        return views;
    }

    public static double SumRollingLaptopExceptionView(RollingLaptopExceptionView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedSequence + parsedMaximum;
    }
}
