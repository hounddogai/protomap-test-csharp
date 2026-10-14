namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay template is in its life cycle.</summary>
public enum RetroPayTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A retro pay template as the retro pay team keeps it.</summary>
public sealed class RetroPayTemplate(string code, string name, string description, double weight, string timezone, List<string> tags, int retries, double amount, string phone, bool archived, RetroPayTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public double Weight { get; set; } = weight;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public string Phone { get; set; } = phone;
    public bool Archived { get; set; } = archived;
    public RetroPayTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay template in status proposed.</summary>
    public static RetroPayTemplate NewRetroPayTemplate(string code, string name)
    {
        return new RetroPayTemplate(
            code,
            name,
            "",
            0.0,
            "",
            new List<string>(),
            0,
            0.0,
            "",
            false,
            RetroPayTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayTemplateStatus next)
    {
        switch (Status)
        {
            case RetroPayTemplateStatus.Proposed:
                switch (next)
                {
                    case RetroPayTemplateStatus.Approved:
                    case RetroPayTemplateStatus.Rejected:
                        return true;
                }
                return false;
            case RetroPayTemplateStatus.Approved:
                return next == RetroPayTemplateStatus.Withdrawn;
            case RetroPayTemplateStatus.Rejected:
                switch (next)
                {
                    case RetroPayTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"retro pay template"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Retries - Weight, 2);
    }

    public RetroPayTemplate Copy()
    {
        return new RetroPayTemplate(
            Code,
            Name,
            Description,
            Weight,
            Timezone,
            Tags,
            Retries,
            Amount,
            Phone,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
