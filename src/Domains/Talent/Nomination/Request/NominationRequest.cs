namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination request is in its life cycle.</summary>
public enum NominationRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A nomination request as the nomination team keeps it.</summary>
public sealed class NominationRequest(string code, string title, int expiresAt, string skill, double discount, NominationRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Skill { get; set; } = skill;
    public double Discount { get; set; } = discount;
    public NominationRequestStatus Status { get; set; } = status;

    /// <summary>Starts a nomination request in status new.</summary>
    public static NominationRequest NewNominationRequest(string code, string title)
    {
        return new NominationRequest(code, title, 0, "", 0.0, NominationRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationRequestStatus next)
    {
        switch (Status)
        {
            case NominationRequestStatus.New:
                switch (next)
                {
                    case NominationRequestStatus.Done:
                    case NominationRequestStatus.Failed:
                        return true;
                }
                return false;
            case NominationRequestStatus.Queued:
                switch (next)
                {
                    case NominationRequestStatus.Done:
                    case NominationRequestStatus.Failed:
                        return true;
                }
                return false;
            case NominationRequestStatus.Running:
                switch (next)
                {
                    case NominationRequestStatus.Failed:
                        return true;
                }
                return false;
            case NominationRequestStatus.Done:
                switch (next)
                {
                    case NominationRequestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"nomination request"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)ExpiresAt + Discount, 2);
    }

    public NominationRequest Copy()
    {
        return new NominationRequest(Code, Title, ExpiresAt, Skill, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
