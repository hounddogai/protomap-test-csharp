namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default vendor score is in its life cycle.</summary>
public enum DefaultVendorScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One default vendor score, with what the planners need to act on it.</summary>
public sealed class DefaultVendorScore(string code, string name, double weight, double maximum, string jobCode, double discount, DefaultVendorScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public double Maximum { get; set; } = maximum;
    public string JobCode { get; set; } = jobCode;
    public double Discount { get; set; } = discount;
    public DefaultVendorScoreStatus Status { get; set; } = status;

    /// <summary>Starts a default vendor score in status draft.</summary>
    public static DefaultVendorScore NewDefaultVendorScore(string code, string name)
    {
        return new DefaultVendorScore(code, name, 0.0, 0.0, "", 0.0, DefaultVendorScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultVendorScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultVendorScoreStatus next)
    {
        switch (Status)
        {
            case DefaultVendorScoreStatus.Draft:
                return next == DefaultVendorScoreStatus.Suspended || next == DefaultVendorScoreStatus.Closed;
            case DefaultVendorScoreStatus.Pending:
                return next == DefaultVendorScoreStatus.Suspended || next == DefaultVendorScoreStatus.Closed;
            case DefaultVendorScoreStatus.Active:
                switch (next)
                {
                    case DefaultVendorScoreStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultVendorScoreStatus.Suspended:
                return next == DefaultVendorScoreStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultVendorScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default vendor score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default vendor score"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Weight - Maximum, 2);
    }

    public DefaultVendorScore Copy()
    {
        return new DefaultVendorScore(Code, Name, Weight, Maximum, JobCode, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
