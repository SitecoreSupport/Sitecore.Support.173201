namespace Sitecore.Support.Events
{
  using System;

  using Sitecore.Diagnostics;
  using Sitecore.Events;
  using Sitecore.SecurityModel;
  using Sitecore.Support.Publishing.Pipelines.PublishItem;

  public class ItemEventHandler
  {
    public void OnItemCloneAdded([CanBeNull] object o, [NotNull] EventArgs e)
    {
      Assert.ArgumentNotNull(e, nameof(e));

      using (new SecurityDisabler())
      {
        var item = SitecoreEventArgs.GetItem(e, 0);

        if (string.IsNullOrEmpty(item[UpdateStatistics.FirstPublishedFieldID]))
        {
          return;
        }

        item.Editing.BeginEdit();
        item[UpdateStatistics.FirstPublishedFieldID] = "";
        item.Editing.EndEdit();
      }
    }
  }
}