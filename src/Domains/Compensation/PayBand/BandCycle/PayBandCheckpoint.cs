namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band checkpoint is in its life cycle.</summary>
public enum PayBandCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A pay band checkpoint. Its code is unique within a tenant.</summary>
public sealed class PayBandCheckpoint(string code, string name, double amount, string segment, int endsAt, string jobCode, double budget, double ratio, PayBandCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Segment { get; set; } = segment;
    public int EndsAt { get; set; } = endsAt;
    public string JobCode { get; set; } = jobCode;
    public double Budget { get; set; } = budget;
    public double Ratio { get; set; } = ratio;
    public PayBandCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a pay band checkpoint in status created.</summary>
    public static PayBandCheckpoint NewPayBandCheckpoint(string code, string name)
    {
        return new PayBandCheckpoint(code, name, 0.0, "", 0, "", 0.0, 0.0, PayBandCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandCheckpointStatus next)
    {
        switch (Status)
        {
            case PayBandCheckpointStatus.Created:
                return next == PayBandCheckpointStatus.Validated || next == PayBandCheckpointStatus.Published;
            case PayBandCheckpointStatus.Validated:
                switch (next)
                {
                    case PayBandCheckpointStatus.Published:
                    case PayBandCheckpointStatus.Retired:
                        return true;
                }
                return false;
            case PayBandCheckpointStatus.Published:
                return next == PayBandCheckpointStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayBandCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Amount - (double)EndsAt, 2);
    }

    public PayBandCheckpoint Copy()
    {
        return new PayBandCheckpoint(Code, Name, Amount, Segment, EndsAt, JobCode, Budget, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
