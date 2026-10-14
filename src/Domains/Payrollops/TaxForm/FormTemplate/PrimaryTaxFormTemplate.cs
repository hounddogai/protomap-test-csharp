namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary tax form template is in its life cycle.</summary>
public enum PrimaryTaxFormTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A primary tax form template as the tax form team keeps it.</summary>
public sealed class PrimaryTaxFormTemplate(string code, string name, double score, string location, double maximum, string employeeId, bool visible, PrimaryTaxFormTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string Location { get; set; } = location;
    public double Maximum { get; set; } = maximum;
    public string EmployeeId { get; set; } = employeeId;
    public bool Visible { get; set; } = visible;
    public PrimaryTaxFormTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a primary tax form template in status created.</summary>
    public static PrimaryTaxFormTemplate NewPrimaryTaxFormTemplate(string code, string name)
    {
        return new PrimaryTaxFormTemplate(code, name, 0.0, "", 0.0, "", false, PrimaryTaxFormTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryTaxFormTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryTaxFormTemplateStatus next)
    {
        switch (Status)
        {
            case PrimaryTaxFormTemplateStatus.Created:
                switch (next)
                {
                    case PrimaryTaxFormTemplateStatus.Validated:
                        return true;
                }
                return false;
            case PrimaryTaxFormTemplateStatus.Validated:
                return next == PrimaryTaxFormTemplateStatus.Published;
            case PrimaryTaxFormTemplateStatus.Published:
                return next == PrimaryTaxFormTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryTaxFormTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary tax form template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Score * Maximum, 2);
    }

    public PrimaryTaxFormTemplate Copy()
    {
        return new PrimaryTaxFormTemplate(Code, Name, Score, Location, Maximum, EmployeeId, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
