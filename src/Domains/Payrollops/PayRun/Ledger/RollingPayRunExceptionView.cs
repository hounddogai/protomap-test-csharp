namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling pay run exception.</summary>
public sealed record RollingPayRunExceptionView(string Code, string Name, string Department, string CeilingText, string NotesText, string Unit, string Approved, string StatusLabel)
{
    public static RollingPayRunExceptionView ToRollingPayRunExceptionView(RollingPayRunException item)
    {
        return new RollingPayRunExceptionView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Department,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Unit,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPayRunExceptionView> ToRollingPayRunExceptionViews(List<RollingPayRunException> items)
    {
        var views = new List<RollingPayRunExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPayRunExceptionView(item));
        }
        return views;
    }

    public static double SumRollingPayRunExceptionView(RollingPayRunExceptionView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
