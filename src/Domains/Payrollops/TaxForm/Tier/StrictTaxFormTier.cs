namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict tax form tier is in its life cycle.</summary>
public enum StrictTaxFormTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict tax form tier. Its code is unique within a tenant.</summary>
public sealed class StrictTaxFormTier(string code, string label, string timezone, int quantity, string skill, int version, StrictTaxFormTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int Quantity { get; set; } = quantity;
    public string Skill { get; set; } = skill;
    public int Version { get; set; } = version;
    public StrictTaxFormTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict tax form tier in status created.</summary>
    public static StrictTaxFormTier NewStrictTaxFormTier(string code, string label)
    {
        return new StrictTaxFormTier(code, label, "", 0, "", 0, StrictTaxFormTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictTaxFormTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictTaxFormTierStatus next)
    {
        switch (Status)
        {
            case StrictTaxFormTierStatus.Created:
                return next == StrictTaxFormTierStatus.Retired;
            case StrictTaxFormTierStatus.Validated:
                switch (next)
                {
                    case StrictTaxFormTierStatus.Retired:
                        return true;
                }
                return false;
            case StrictTaxFormTierStatus.Published:
                switch (next)
                {
                    case StrictTaxFormTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictTaxFormTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict tax form tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Quantity + (double)Version, 2);
    }

    public StrictTaxFormTier Copy()
    {
        return new StrictTaxFormTier(Code, Label, Timezone, Quantity, Skill, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
