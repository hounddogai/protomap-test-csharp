namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount profile is in its life cycle.</summary>
public enum HeadcountProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a headcount profile and its current status.</summary>
public sealed class HeadcountProfile(string code, string label, string ownerId, string locale, double amount, string managerId, string timezone, HeadcountProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Locale { get; set; } = locale;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public string Timezone { get; set; } = timezone;
    public HeadcountProfileStatus Status { get; set; } = status;

    /// <summary>Starts a headcount profile in status idle.</summary>
    public static HeadcountProfile NewHeadcountProfile(string code, string label)
    {
        return new HeadcountProfile(code, label, "", "", 0.0, "", "", HeadcountProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountProfileStatus next)
    {
        switch (Status)
        {
            case HeadcountProfileStatus.Idle:
                switch (next)
                {
                    case HeadcountProfileStatus.Warming:
                        return true;
                }
                return false;
            case HeadcountProfileStatus.Warming:
                switch (next)
                {
                    case HeadcountProfileStatus.Ready:
                    case HeadcountProfileStatus.Draining:
                        return true;
                }
                return false;
            case HeadcountProfileStatus.Ready:
                switch (next)
                {
                    case HeadcountProfileStatus.Draining:
                        return true;
                }
                return false;
            case HeadcountProfileStatus.Draining:
                switch (next)
                {
                    case HeadcountProfileStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"headcount profile"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public HeadcountProfile Copy()
    {
        return new HeadcountProfile(Code, Label, OwnerId, Locale, Amount, ManagerId, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
