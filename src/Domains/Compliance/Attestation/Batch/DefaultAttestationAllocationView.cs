namespace Acme.Workforce.Compliance.Attestation.Batch;

using System.Globalization;

/// <summary>How screens and exports show a default attestation allocation.</summary>
public sealed record DefaultAttestationAllocationView(string Code, string Title, string Visible, string ManagerId, string Ceiling, string Badge, string EmployeeIdText, string GraceDays, string StatusLabel)
{
    public static DefaultAttestationAllocationView ToDefaultAttestationAllocationView(DefaultAttestationAllocation item)
    {
        return new DefaultAttestationAllocationView(
            item.Code,
            item.Title,
            $"{item.Visible}",
            item.ManagerId,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.EmployeeId,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAttestationAllocationView> ToDefaultAttestationAllocationViews(List<DefaultAttestationAllocation> items)
    {
        var views = new List<DefaultAttestationAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAttestationAllocationView(item));
        }
        return views;
    }

    public static double SumDefaultAttestationAllocationView(DefaultAttestationAllocationView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedGraceDays;
    }
}
