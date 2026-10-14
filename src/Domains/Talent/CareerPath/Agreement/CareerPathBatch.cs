namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path batch is in its life cycle.</summary>
public enum CareerPathBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a career path batch and its current status.</summary>
public sealed class CareerPathBatch(string code, string name, string category, string summary, bool active, double minimum, string title, List<string> tags, string costCenter, string grade, string team, CareerPathBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Summary { get; set; } = summary;
    public bool Active { get; set; } = active;
    public double Minimum { get; set; } = minimum;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string CostCenter { get; set; } = costCenter;
    public string Grade { get; set; } = grade;
    public string Team { get; set; } = team;
    public CareerPathBatchStatus Status { get; set; } = status;

    /// <summary>Starts a career path batch in status proposed.</summary>
    public static CareerPathBatch NewCareerPathBatch(string code, string name)
    {
        return new CareerPathBatch(
            code,
            name,
            "",
            "",
            false,
            0.0,
            "",
            new List<string>(),
            "",
            "",
            "",
            CareerPathBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathBatchStatus next)
    {
        switch (Status)
        {
            case CareerPathBatchStatus.Proposed:
                switch (next)
                {
                    case CareerPathBatchStatus.Approved:
                        return true;
                }
                return false;
            case CareerPathBatchStatus.Approved:
                switch (next)
                {
                    case CareerPathBatchStatus.Withdrawn:
                        return true;
                }
                return false;
            case CareerPathBatchStatus.Rejected:
                return next == CareerPathBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CareerPathBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"career path batch"} {Code}";
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

    public double MeasuredValue()
    {
        return Minimum;
    }

    public CareerPathBatch Copy()
    {
        return new CareerPathBatch(
            Code,
            Name,
            Category,
            Summary,
            Active,
            Minimum,
            Title,
            Tags,
            CostCenter,
            Grade,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
