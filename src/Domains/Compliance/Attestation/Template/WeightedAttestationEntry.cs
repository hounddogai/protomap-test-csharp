namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted attestation entry is in its life cycle.</summary>
public enum WeightedAttestationEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One weighted attestation entry, with what the planners need to act on it.</summary>
public sealed class WeightedAttestationEntry(string code, string label, string segment, int capacity, int batchSize, string jobCode, WeightedAttestationEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int Capacity { get; set; } = capacity;
    public int BatchSize { get; set; } = batchSize;
    public string JobCode { get; set; } = jobCode;
    public WeightedAttestationEntryStatus Status { get; set; } = status;

    /// <summary>Starts a weighted attestation entry in status idle.</summary>
    public static WeightedAttestationEntry NewWeightedAttestationEntry(string code, string label)
    {
        return new WeightedAttestationEntry(code, label, "", 0, 0, "", WeightedAttestationEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAttestationEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAttestationEntryStatus next)
    {
        switch (Status)
        {
            case WeightedAttestationEntryStatus.Idle:
                switch (next)
                {
                    case WeightedAttestationEntryStatus.Ready:
                        return true;
                }
                return false;
            case WeightedAttestationEntryStatus.Warming:
                return next == WeightedAttestationEntryStatus.Ready || next == WeightedAttestationEntryStatus.Stopped;
            case WeightedAttestationEntryStatus.Ready:
                switch (next)
                {
                    case WeightedAttestationEntryStatus.Draining:
                    case WeightedAttestationEntryStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedAttestationEntryStatus.Draining:
                switch (next)
                {
                    case WeightedAttestationEntryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAttestationEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted attestation entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)BatchSize * (double)Capacity, 2);
    }

    public WeightedAttestationEntry Copy()
    {
        return new WeightedAttestationEntry(Code, Label, Segment, Capacity, BatchSize, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
