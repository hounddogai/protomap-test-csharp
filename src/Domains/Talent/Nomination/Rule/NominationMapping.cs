namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination mapping is in its life cycle.</summary>
public enum NominationMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A nomination mapping as the nomination team keeps it.</summary>
public sealed class NominationMapping(string code, string name, string phone, int attempts, int sequence, NominationMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public int Attempts { get; set; } = attempts;
    public int Sequence { get; set; } = sequence;
    public NominationMappingStatus Status { get; set; } = status;

    /// <summary>Starts a nomination mapping in status requested.</summary>
    public static NominationMapping NewNominationMapping(string code, string name)
    {
        return new NominationMapping(code, name, "", 0, 0, NominationMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationMappingStatus next)
    {
        switch (Status)
        {
            case NominationMappingStatus.Requested:
                switch (next)
                {
                    case NominationMappingStatus.Revoked:
                        return true;
                }
                return false;
            case NominationMappingStatus.Granted:
                switch (next)
                {
                    case NominationMappingStatus.Revoked:
                    case NominationMappingStatus.Expired:
                        return true;
                }
                return false;
            case NominationMappingStatus.Revoked:
                return next == NominationMappingStatus.Expired;
        }
        return false;
    }

    public void MoveTo(NominationMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Attempts + (double)Sequence, 2);
    }

    public NominationMapping Copy()
    {
        return new NominationMapping(Code, Name, Phone, Attempts, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
