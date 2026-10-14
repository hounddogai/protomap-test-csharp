namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack score is in its life cycle.</summary>
public enum PolicyAckScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A policy ack score as the policy ack team keeps it.</summary>
public sealed class PolicyAckScore(string code, string label, string checksum, int graceDays, string country, double fte, double rate, double minimum, double cost, int updatedAt, PolicyAckScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public double Fte { get; set; } = fte;
    public double Rate { get; set; } = rate;
    public double Minimum { get; set; } = minimum;
    public double Cost { get; set; } = cost;
    public int UpdatedAt { get; set; } = updatedAt;
    public PolicyAckScoreStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack score in status new.</summary>
    public static PolicyAckScore NewPolicyAckScore(string code, string label)
    {
        return new PolicyAckScore(code, label, "", 0, "", 0.0, 0.0, 0.0, 0.0, 0, PolicyAckScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckScoreStatus next)
    {
        switch (Status)
        {
            case PolicyAckScoreStatus.New:
                switch (next)
                {
                    case PolicyAckScoreStatus.Queued:
                    case PolicyAckScoreStatus.Failed:
                        return true;
                }
                return false;
            case PolicyAckScoreStatus.Queued:
                switch (next)
                {
                    case PolicyAckScoreStatus.Running:
                    case PolicyAckScoreStatus.Done:
                        return true;
                }
                return false;
            case PolicyAckScoreStatus.Running:
                return next == PolicyAckScoreStatus.Done || next == PolicyAckScoreStatus.Failed;
            case PolicyAckScoreStatus.Done:
                switch (next)
                {
                    case PolicyAckScoreStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Cost + (double)UpdatedAt, 2);
    }

    public PolicyAckScore Copy()
    {
        return new PolicyAckScore(
            Code,
            Label,
            Checksum,
            GraceDays,
            Country,
            Fte,
            Rate,
            Minimum,
            Cost,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
