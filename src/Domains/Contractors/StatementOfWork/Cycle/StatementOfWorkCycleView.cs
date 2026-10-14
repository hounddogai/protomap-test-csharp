namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work cycle.</summary>
public sealed record StatementOfWorkCycleView(string Code, string LabelText, string SalaryBandText, string Title, string Checksum, string MinimumText, string FteText, string Phone, string Discount, string SkillText, string StatusLabel)
{
    public static StatementOfWorkCycleView ToStatementOfWorkCycleView(StatementOfWorkCycle item)
    {
        return new StatementOfWorkCycleView(
            item.Code,
            item.Label,
            item.SalaryBand,
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Checksum, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkCycleView> ToStatementOfWorkCycleViews(List<StatementOfWorkCycle> items)
    {
        var views = new List<StatementOfWorkCycleView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkCycleView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkCycleView(StatementOfWorkCycleView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedFte + parsedDiscount;
    }
}
