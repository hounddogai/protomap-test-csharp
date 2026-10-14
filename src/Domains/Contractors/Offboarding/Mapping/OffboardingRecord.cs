namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding record is in its life cycle.</summary>
public enum OffboardingRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a offboarding record and its current status.</summary>
public sealed class OffboardingRecord(string code, string label, Dictionary<string, string> attributes, int tenureMonths, double ratio, OffboardingRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ratio { get; set; } = ratio;
    public OffboardingRecordStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding record in status requested.</summary>
    public static OffboardingRecord NewOffboardingRecord(string code, string label)
    {
        return new OffboardingRecord(
            code,
            label,
            new Dictionary<string, string>(),
            0,
            0.0,
            OffboardingRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingRecordStatus next)
    {
        switch (Status)
        {
            case OffboardingRecordStatus.Requested:
                switch (next)
                {
                    case OffboardingRecordStatus.Granted:
                        return true;
                }
                return false;
            case OffboardingRecordStatus.Granted:
                return next == OffboardingRecordStatus.Revoked;
            case OffboardingRecordStatus.Revoked:
                switch (next)
                {
                    case OffboardingRecordStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetVolume()
    {
        return Math.Round(Ratio - (double)TenureMonths, 2);
    }

    public OffboardingRecord Copy()
    {
        return new OffboardingRecord(Code, Label, Attributes, TenureMonths, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
