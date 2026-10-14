namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty profile is in its life cycle.</summary>
public enum WarrantyProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A warranty profile. Its code is unique within a tenant.</summary>
public sealed class WarrantyProfile(string code, string title, double hours, string name, double threshold, string salaryBand, WarrantyProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string SalaryBand { get; set; } = salaryBand;
    public WarrantyProfileStatus Status { get; set; } = status;

    /// <summary>Starts a warranty profile in status created.</summary>
    public static WarrantyProfile NewWarrantyProfile(string code, string title)
    {
        return new WarrantyProfile(code, title, 0.0, "", 0.0, "", WarrantyProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyProfileStatus next)
    {
        switch (Status)
        {
            case WarrantyProfileStatus.Created:
                switch (next)
                {
                    case WarrantyProfileStatus.Published:
                    case WarrantyProfileStatus.Retired:
                        return true;
                }
                return false;
            case WarrantyProfileStatus.Validated:
                return next == WarrantyProfileStatus.Retired;
            case WarrantyProfileStatus.Published:
                switch (next)
                {
                    case WarrantyProfileStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Threshold * Hours, 2);
    }

    public WarrantyProfile Copy()
    {
        return new WarrantyProfile(Code, Title, Hours, Name, Threshold, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
