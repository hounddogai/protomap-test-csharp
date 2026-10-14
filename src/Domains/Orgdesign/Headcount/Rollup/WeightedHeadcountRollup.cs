namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted headcount rollup is in its life cycle.</summary>
public enum WeightedHeadcountRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A weighted headcount rollup. Its code is unique within a tenant.</summary>
public sealed class WeightedHeadcountRollup(string code, string label, string location, bool pinned, string contactEmail, int quantity, WeightedHeadcountRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public bool Pinned { get; set; } = pinned;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public WeightedHeadcountRollupStatus Status { get; set; } = status;

    /// <summary>Starts a weighted headcount rollup in status draft.</summary>
    public static WeightedHeadcountRollup NewWeightedHeadcountRollup(string code, string label)
    {
        return new WeightedHeadcountRollup(code, label, "", false, "", 0, WeightedHeadcountRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedHeadcountRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedHeadcountRollupStatus next)
    {
        switch (Status)
        {
            case WeightedHeadcountRollupStatus.Draft:
                switch (next)
                {
                    case WeightedHeadcountRollupStatus.Suspended:
                        return true;
                }
                return false;
            case WeightedHeadcountRollupStatus.Pending:
                return next == WeightedHeadcountRollupStatus.Suspended;
            case WeightedHeadcountRollupStatus.Active:
                return next == WeightedHeadcountRollupStatus.Closed;
            case WeightedHeadcountRollupStatus.Suspended:
                return next == WeightedHeadcountRollupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WeightedHeadcountRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted headcount rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted headcount rollup"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public WeightedHeadcountRollup Copy()
    {
        return new WeightedHeadcountRollup(Code, Label, Location, Pinned, ContactEmail, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
