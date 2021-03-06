﻿Copyright (C) The FARM Digital Australia p/l

This package adds a new data type to your Umbraco installation: the Embedded Content data type. In short it allows you to add content to a node just in the same way as you would do with 'sub nodes', e.g. a list of specifications for a product. Instead of creating a document type called specification (without template) that contains 2 properties, name and value, you would create a new Embedded Content data type, add 2 'properties' name and value of type textstring to it and place the data type on your product doc type. You can now add as many name-value-entries to the list as you like, it will be stored as Xml data in the backend. 
The data types which are currently available are
- Textstring
- Textbox multiple
- True/false
- Content picker
- Media picker
- Simple editor
- Date picker

After installing the package you will first have to create a new data type with a name of your choice, and then set the data type to use to 'Embedded Content'. After saving you will be able to add custom properties to the data type, you can also edit, delete or re-order them. Once you are happy with the definition you can then use the data type on a document type and add content on a content node.

Please make sure the following entries are in the web.config file, especially if you are installing manually:
&lt;configuration&gt;
  &lt;!-- for IIS6 --&gt;
  &lt;system.web&gt;
    &lt;httpHandlers&gt;
      &lt;add path="umbraco/REST/umbraconodename" verb="*" type="TheFarm.Umbraco.EmbeddedContent.UmbracoNodeNameHandler, TheFarm.Umbraco.EmbeddedContent" validate="true" /&gt;
    &lt;/httpHandlers&gt;
  &lt;/system.web&gt;
  &lt;!-- for IIS 7 --&gt;
  &lt;system.webServer&gt;
    &lt;handlers&gt;
      &lt;remove name="UmbracoNodeName" /&gt;
      &lt;add name="UmbracoNodeName" path="umbraco/REST/umbraconodename" verb="*" type="TheFarm.Umbraco.EmbeddedContent.UmbracoNodeNameHandler, TheFarm.Umbraco.EmbeddedContent" preCondition="integratedMode" /&gt;
    &lt;/handlers&gt;
  &lt;/system.webServer&gt;
&lt;/configuration&gt;


Version History:
1.1.3: Fixed a bug where the correct value wasn't read when adding a new content item.
1.1.2: A couple of bug fixes, most notably when another control on the page does a post back. Added 'max number of items' field which let's you limit the number of content items (credits go to Matt Brailsford for the idea).
1.1.1: Bug fixes: 
- special characters in textstring and textbox multiple
- fixed error message when saving a content node with an empty Embedded Content control
- fixed error after re-ordering items with special characters
Additionally the first element in the add box will now be selected when clicking on 'add' or 'edit'.
1.1: Couple of bug fixes, a new prevalue schema, VALIDATION and a lot of VISUAL IMPROVEMENTS :)
1.09 Added a backup web service in case the node name handler doesn't work
1.08 Fixed 2 issues with the content editor (thanks Keith!)
1.07 Added html encoding to the description field (thanks James!) and fixed an issue where aliases where all lowercased when re-ordering (thanks Michael!)
1.06 Completed JS refactoring, upgraded jquery.dragsort library to fix IE issues
1.05 Data from the simple editor will now be saved in a CDATA section.
1.04 Fixed a bug when deleting an item from the list and compiled against .Net 3.5 so it runs with this framework as well!
1.03 Fixed a bug that occured with 2 of the same data types on one doc type and implemented graphical improvements (credits to Petr Snobelt!)
1.02 Added delete confirmations for both content and prevalue editor
1.01 Added new function 'showLabel' + bug fixes
1.0 Initial release

This data type is similar to the Repeatable Custom Content one (http://our.umbraco.org/projects/backoffice-extensions/repeatable-custom-content) made by Masood Afzal (http://umbraco.masoodafzal.com/) which is available for Umbraco v 4.0.*, and although this is a complete rebuild targeted at Umbraco v 4.5+ I'd like to give some credits over in his direction. :)