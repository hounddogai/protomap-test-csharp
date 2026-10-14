namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg threshold is in its life cycle.</summary>
public enum ReorgThresholdStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A reorg threshold. Its code is unique within a tenant.</summary>
public sealed class ReorgThreshold(string code, string title, string salaryBand, string reviewer, double discount, string externalRef, double cost, string channel, ReorgThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string Reviewer { get; set; } = reviewer;
    public double Discount { get; set; } = discount;
    public string ExternalRef { get; set; } = externalRef;
    public double Cost { get; set; } = cost;
    public string Channel { get; set; } = channel;
    public ReorgThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a reorg threshold in status created.</summary>
    public static ReorgThreshold NewReorgThreshold(string code, string title)
    {
        return new ReorgThreshold(code, title, "", "", 0.0, "", 0.0, "", ReorgThresholdStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgThresholdStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgThresholdStatus next)
    {
        switch (Status)
        {
            case ReorgThresholdStatus.Created:
                switch (next)
                {
                    case ReorgThresholdStatus.Retired:
                        return true;
                }
                return false;
            case ReorgThresholdStatus.Validated:
                return next == ReorgThresholdStatus.Retired;
            case ReorgThresholdStatus.Published:
                switch (next)
                {
                    case ReorgThresholdStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetVolume()
    {
        return Math.Round(Discount * Cost, 2);
    }

    public ReorgThreshold Copy()
    {
        return new ReorgThreshold(Code, Title, SalaryBand, Reviewer, Discount, ExternalRef, Cost, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
