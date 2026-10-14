namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted attestation notice is in its life cycle.</summary>
public enum WeightedAttestationNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One weighted attestation notice, with what the planners need to act on it.</summary>
public sealed class WeightedAttestationNotice(string code, string name, string channel, string managerId, double rate, WeightedAttestationNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public WeightedAttestationNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a weighted attestation notice in status proposed.</summary>
    public static WeightedAttestationNotice NewWeightedAttestationNotice(string code, string name)
    {
        return new WeightedAttestationNotice(code, name, "", "", 0.0, WeightedAttestationNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAttestationNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAttestationNoticeStatus next)
    {
        switch (Status)
        {
            case WeightedAttestationNoticeStatus.Proposed:
                switch (next)
                {
                    case WeightedAttestationNoticeStatus.Rejected:
                    case WeightedAttestationNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedAttestationNoticeStatus.Approved:
                switch (next)
                {
                    case WeightedAttestationNoticeStatus.Rejected:
                        return true;
                }
                return false;
            case WeightedAttestationNoticeStatus.Rejected:
                return next == WeightedAttestationNoticeStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedAttestationNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted attestation notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted attestation notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public WeightedAttestationNotice Copy()
    {
        return new WeightedAttestationNotice(Code, Name, Channel, ManagerId, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
