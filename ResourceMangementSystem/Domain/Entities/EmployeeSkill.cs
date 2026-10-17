using ResourceMangementSystem.Domain.Enums;

namespace ResourceMangementSystem.Domain.Entities;

public sealed class EmployeeSkill
{
    public string SkillName { get; private set; }

    public ProficiencyLevel Level { get; private set; }

    public EmployeeSkill(string skillName, ProficiencyLevel level)
    {
        if (string.IsNullOrWhiteSpace(skillName))
        {
            throw new ArgumentException("Skill name is required.", nameof(skillName));
        }

        SkillName = skillName.Trim();
        Level = level;
    }

    public void UpdateLevel(ProficiencyLevel level)
    {
        Level = level;
    }
}
