namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey digest is in its life cycle.</summary>
public enum PulseSurveyDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One pulse survey digest, with what the planners need to act on it.</summary>
public sealed class PulseSurveyDigest(string code, string title, string channel, string unit, string checksum, double weight, string managerId, string employeeId, string label, int revision, PulseSurveyDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public string ManagerId { get; set; } = managerId;
    public string EmployeeId { get; set; } = employeeId;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public PulseSurveyDigestStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey digest in status draft.</summary>
    public static PulseSurveyDigest NewPulseSurveyDigest(string code, string title)
    {
        return new PulseSurveyDigest(code, title, "", "", "", 0.0, "", "", "", 0, PulseSurveyDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyDigestStatus next)
    {
        switch (Status)
        {
            case PulseSurveyDigestStatus.Draft:
                switch (next)
                {
                    case PulseSurveyDigestStatus.Pending:
                    case PulseSurveyDigestStatus.Suspended:
                        return true;
                }
                return false;
            case PulseSurveyDigestStatus.Pending:
                switch (next)
                {
                    case PulseSurveyDigestStatus.Active:
                    case PulseSurveyDigestStatus.Closed:
                        return true;
                }
                return false;
            case PulseSurveyDigestStatus.Active:
                switch (next)
                {
                    case PulseSurveyDigestStatus.Suspended:
                    case PulseSurveyDigestStatus.Closed:
                        return true;
                }
                return false;
            case PulseSurveyDigestStatus.Suspended:
                switch (next)
                {
                    case PulseSurveyDigestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pulse survey digest"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Revision * Weight, 2);
    }

    public PulseSurveyDigest Copy()
    {
        return new PulseSurveyDigest(
            Code,
            Title,
            Channel,
            Unit,
            Checksum,
            Weight,
            ManagerId,
            EmployeeId,
            Label,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
