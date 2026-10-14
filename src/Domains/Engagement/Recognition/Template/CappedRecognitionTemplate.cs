namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped recognition template is in its life cycle.</summary>
public enum CappedRecognitionTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a capped recognition template and its current status.</summary>
public sealed class CappedRecognitionTemplate(string code, string name, string summary, int revision, double rate, string region, double fte, string contactEmail, double rating, double weight, int capacity, CappedRecognitionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public int Revision { get; set; } = revision;
    public double Rate { get; set; } = rate;
    public string Region { get; set; } = region;
    public double Fte { get; set; } = fte;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rating { get; set; } = rating;
    public double Weight { get; set; } = weight;
    public int Capacity { get; set; } = capacity;
    public CappedRecognitionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a capped recognition template in status draft.</summary>
    public static CappedRecognitionTemplate NewCappedRecognitionTemplate(string code, string name)
    {
        return new CappedRecognitionTemplate(
            code,
            name,
            "",
            0,
            0.0,
            "",
            0.0,
            "",
            0.0,
            0.0,
            0,
            CappedRecognitionTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRecognitionTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRecognitionTemplateStatus next)
    {
        switch (Status)
        {
            case CappedRecognitionTemplateStatus.Draft:
                return next == CappedRecognitionTemplateStatus.Active || next == CappedRecognitionTemplateStatus.Closed;
            case CappedRecognitionTemplateStatus.Pending:
                return next == CappedRecognitionTemplateStatus.Active || next == CappedRecognitionTemplateStatus.Closed;
            case CappedRecognitionTemplateStatus.Active:
                switch (next)
                {
                    case CappedRecognitionTemplateStatus.Suspended:
                    case CappedRecognitionTemplateStatus.Closed:
                        return true;
                }
                return false;
            case CappedRecognitionTemplateStatus.Suspended:
                switch (next)
                {
                    case CappedRecognitionTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedRecognitionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped recognition template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"capped recognition template"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Weight * (double)Revision, 2);
    }

    public CappedRecognitionTemplate Copy()
    {
        return new CappedRecognitionTemplate(
            Code,
            Name,
            Summary,
            Revision,
            Rate,
            Region,
            Fte,
            ContactEmail,
            Rating,
            Weight,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
