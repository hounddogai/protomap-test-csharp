namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band entry is in its life cycle.</summary>
public enum PayBandEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pay band entry, with what the planners need to act on it.</summary>
public sealed class PayBandEntry(string code, string title, int batchSize, string department, int retries, string checksum, Dictionary<string, string> attributes, PayBandEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Department { get; set; } = department;
    public int Retries { get; set; } = retries;
    public string Checksum { get; set; } = checksum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public PayBandEntryStatus Status { get; set; } = status;

    /// <summary>Starts a pay band entry in status idle.</summary>
    public static PayBandEntry NewPayBandEntry(string code, string title)
    {
        return new PayBandEntry(
            code,
            title,
            0,
            "",
            0,
            "",
            new Dictionary<string, string>(),
            PayBandEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandEntryStatus next)
    {
        switch (Status)
        {
            case PayBandEntryStatus.Idle:
                return next == PayBandEntryStatus.Warming || next == PayBandEntryStatus.Ready;
            case PayBandEntryStatus.Warming:
                return next == PayBandEntryStatus.Ready || next == PayBandEntryStatus.Stopped;
            case PayBandEntryStatus.Ready:
                return next == PayBandEntryStatus.Draining;
            case PayBandEntryStatus.Draining:
                return next == PayBandEntryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayBandEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)Retries * (double)BatchSize, 2);
    }

    public PayBandEntry Copy()
    {
        return new PayBandEntry(Code, Title, BatchSize, Department, Retries, Checksum, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
