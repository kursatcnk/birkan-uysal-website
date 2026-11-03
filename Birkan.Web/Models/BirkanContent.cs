namespace Birkan.Web.Models;

// Sitedeki her metin ve görsel bu modelden okunuyor; panel de yalnızca bu modeli düzenliyor.
public sealed class BirkanContent
{
    public BirkanSiteInfo Site { get; set; } = new();
    public BirkanHero Hero { get; set; } = new();
    public BirkanAbout About { get; set; } = new();
    public BirkanSection<BirkanEducation> Education { get; set; } = new();
    public BirkanSection<BirkanExperience> Experience { get; set; } = new();
    public BirkanSection<BirkanSkill> Skills { get; set; } = new();
    public BirkanSection<BirkanProject> Projects { get; set; } = new();
    public BirkanSection<BirkanContactItem> Contact { get; set; } = new();
}

public sealed class BirkanSiteInfo
{
    public string Title { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string Logo { get; set; } = "";
    public string Avatar { get; set; } = "";
    public BirkanNavLabels Nav { get; set; } = new();
}

public sealed class BirkanNavLabels
{
    public string Home { get; set; } = "";
    public string About { get; set; } = "";
    public string Education { get; set; } = "";
    public string Experience { get; set; } = "";
    public string Skills { get; set; } = "";
    public string Projects { get; set; } = "";
    public string Contact { get; set; } = "";
}

public sealed class BirkanHero
{
    public string Name { get; set; } = "";
    public string Text { get; set; } = "";
    public string Photo { get; set; } = "";
    public List<BirkanSocialLink> Socials { get; set; } = [];
}

public sealed class BirkanSocialLink
{
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
}

public sealed class BirkanAbout
{
    public string Title { get; set; } = "";
    public string Greeting { get; set; } = "";
    public string GreetingName { get; set; } = "";
    public string Bio { get; set; } = "";
    public string ExpertiseTitle { get; set; } = "";
    public string ExpertiseDescription { get; set; } = "";
    public List<string> Expertise { get; set; } = [];
    public string Photo { get; set; } = "";
    public string BadgeTop { get; set; } = "";
    public string BadgeBottom { get; set; } = "";
}

public sealed class BirkanSection<T>
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<T> Items { get; set; } = [];
}

public sealed class BirkanEducation
{
    public string School { get; set; } = "";
    public string Faculty { get; set; } = "";
    public string Department { get; set; } = "";
    public string Degree { get; set; } = "";
    public string Years { get; set; } = "";
    public string Logo { get; set; } = "";
}

public sealed class BirkanExperience
{
    public string Company { get; set; } = "";
    public string Position { get; set; } = "";
    public string Location { get; set; } = "";
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
    public string Logo { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Highlights { get; set; } = [];
}

public sealed class BirkanSkill
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
}

public sealed class BirkanProject
{
    public string Badge { get; set; } = "";
    public string Title { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public string Hint { get; set; } = "";
    public string Intro { get; set; } = "";
    public string Image { get; set; } = "";
    public List<BirkanProjectBlock> Blocks { get; set; } = [];
    public string ListTitle { get; set; } = "";
    public List<string> Highlights { get; set; } = [];
}

public sealed class BirkanProjectBlock
{
    public string Heading { get; set; } = "";
    public string Text { get; set; } = "";
}

public sealed class BirkanContactItem
{
    public string Type { get; set; } = "";
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Url { get; set; } = "";
}
