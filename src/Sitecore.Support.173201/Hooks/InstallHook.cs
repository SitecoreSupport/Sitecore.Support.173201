namespace Sitecore.Support.Hooks
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Events.Hooks;
  using Sitecore.SecurityModel;
  using Sitecore.Support.Publishing.Pipelines.PublishItem;

  [UsedImplicitly]
  public class InstallHook : IHook
  {
    public void Initialize()
    {
      var database = Factory.GetDatabase("master");
      if (database == null)
      {
        return;
      }

      using (new SecurityDisabler())
      {
        var statisticsSection = database.GetItem(FieldIDs.Revision)?.Parent;
        Assert.IsNotNull(statisticsSection, nameof(statisticsSection));

        EnsureItem(
          itemId: UpdateStatistics.FirstPublishedFieldID,
          itemName: "__Published First",
          title: "Published First",
          sortorder: "1000",
          statisticsSection: statisticsSection);
      }
    }

    private static void EnsureItem(ID itemId, string itemName, string title, string sortorder, Item statisticsSection)
    {
      var database = statisticsSection.Database;
      var item = database.GetItem(itemId);
      if (item != null)
      {
        return;
      }

      var thisType = typeof(InstallHook);
      Log.Audit($"Creating item {statisticsSection.Paths.FullPath}/{itemName} ({itemId}) - {thisType.Assembly.GetName().Name}", thisType);

      item = statisticsSection.Add(itemName, new TemplateID(TemplateIDs.TemplateField), itemId);
      Assert.IsNotNull(item, $"Cannot find created item: {itemName} ({itemId})");

      item.Editing.BeginEdit();
      item["Type"] = "datetime";
      item["Title"] = title;
      item["Default Value"] = "";
      item[FieldIDs.Sortorder] = sortorder;
      item[TemplateFieldIDs.Shared] = "1";
      item.Editing.EndEdit();
    }
  }
}