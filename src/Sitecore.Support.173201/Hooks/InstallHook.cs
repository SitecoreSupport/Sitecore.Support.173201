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

        RemoveItem(
          itemName: "__Published Last",
          statisticsSection: statisticsSection);
      }
    }

    private static void EnsureItem([NotNull] ID itemId, [NotNull] string itemName, [NotNull] string title, string sortorder, [NotNull] Item statisticsSection)
    {
      var database = statisticsSection.Database;
      var item = database.GetItem(itemId);
      if (item != null)
      {
        return;
      }
      
      RemoveItem(
        itemName: itemName,
        statisticsSection: statisticsSection);

      Log.Audit($"Creating item {statisticsSection.Paths.FullPath}/{itemName} ({itemId}) - {typeof(InstallHook).Assembly.GetName().Name}", typeof(InstallHook));

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

    private static void RemoveItem([NotNull] string itemName, [NotNull] Item statisticsSection)
    {
      var item = statisticsSection.Children[itemName];
      if (item == null)
      {
        return;
      }

      Log.Audit($"Deleting item {statisticsSection.Paths.FullPath}/{itemName} ({item.ID}) - {typeof(InstallHook).Assembly.GetName().Name}", typeof(InstallHook));

      item.Delete();
    }
  }
}