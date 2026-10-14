namespace Acme.Workforce.Talent.Nomination.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination template is in its life cycle.</summary>
public enum NominationTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One nomination template, with what the planners need to act on it.</summary>
public sealed class NominationTemplate(string code, string name, string grade, double cost, Dictionary<string, string> attributes, double rate, int attempts, double weight, string badge, string checksum, NominationTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public double Cost { get; set; } = cost;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public double Weight { get; set; } = weight;
    public string Badge { get; set; } = badge;
    public string Checksum { get; set; } = checksum;
    public NominationTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a nomination template in status requested.</summary>
    public static NominationTemplate NewNominationTemplate(string code, string name)
    {
        return new NominationTemplate(
            code,
            name,
            "",
            0.0,
            new Dictionary<string, string>(),
            0.0,
            0,
            0.0,
            "",
            "",
            NominationTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationTemplateStatus next)
    {
        switch (Status)
        {
            case NominationTemplateStatus.Requested:
                return next == NominationTemplateStatus.Granted || next == NominationTemplateStatus.Expired;
            case NominationTemplateStatus.Granted:
                return next == NominationTemplateStatus.Revoked || next == NominationTemplateStatus.Expired;
            case NominationTemplateStatus.Revoked:
                return next == NominationTemplateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(NominationTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double WeightedLoad()
    {
        return Math.Round(Rate - (double)Attempts, 2);
    }

    public NominationTemplate Copy()
    {
        return new NominationTemplate(
            Code,
            Name,
            Grade,
            Cost,
            Attributes,
            Rate,
            Attempts,
            Weight,
            Badge,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
