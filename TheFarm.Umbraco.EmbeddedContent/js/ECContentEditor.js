//Compares the number of items in the list to the maxCount variable and enables or disables the add button
TheFarm.Umbraco.EmbeddedContent.CheckMaxCount = function (dtdid) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    if (EC.maxCount == 0) return true;

    valuexml = decodeURIComponent($("input[id$='hiddenXmlValue" + dtdid + "']").val());
    dojo.require("dojox.xml.parser");
    xmlDoc = dojox.xml.parser.parse(valuexml);

    if (xmlDoc.documentElement && xmlDoc.documentElement.hasChildNodes()) {
        count = xmlDoc.documentElement.childNodes.length;
    } else return true;

    return (count < EC.maxCount);
}

TheFarm.Umbraco.EmbeddedContent.AddBlurToFields = function (dtdid) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    schemaxml = decodeURIComponent($("input[id$='EChiddenXmlSchema" + dtdid + "']").val());
    if (window.DOMParser) {
        doc = schemaxml;
    } else // Internet Explorer
    {
        xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
        xmlDoc.async = "false";
        xmlDoc.loadXML(schemaxml);
        doc = $(xmlDoc).children();
    }
    $(doc).children().each(function () {
        id = $(this).attr('propertyid');
        type = $(this).attr('type');
        required = $(this).attr('required');
        validation = $(this).attr('validation');
        EC.AddBlur(dtdid, id, type, required, validation);
    });
    return false;
}

TheFarm.Umbraco.EmbeddedContent.AddBlur = function (dtdid, fieldId, type, required, validation) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    if (type == 'True/false') {
        //do nothing
    } else if (type == 'Content picker' || type == 'Media picker') {
        //does not work!!
        //$("td:has(input[id$='field" + dtdid + "_" + fieldId + "_ContentIdValue']) a").blur(function () {
        //    EC.ValidateField(dtdid, fieldId, type, required, validation);
        //});
    } else if (type == 'Date picker') {
        //no blur action here
    } else if (type == 'Textbox multiple' || type == 'Simple editor') {
        $("textarea[id$='field" + dtdid + "_" + fieldId + "']").blur(function () {
            EC.ValidateField(dtdid, fieldId, type, required, validation);
        });
    } else {
        $("input[id$='field" + dtdid + "_" + fieldId + "']").blur(function () {
            EC.ValidateField(dtdid, fieldId, type, required, validation);
        });
    }
}

TheFarm.Umbraco.EmbeddedContent.ValidateFields = function (dtdid) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    valid = true;
    schemaxml = decodeURIComponent($("input[id$='EChiddenXmlSchema" + dtdid + "']").val());
    if (window.DOMParser) {
        doc = schemaxml;
    } else // Internet Explorer
    {
        xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
        xmlDoc.async = "false";
        xmlDoc.loadXML(schemaxml);
        doc = $(xmlDoc).children();
    }
    $(doc).children().each(function () {
        id = $(this).attr('propertyid');
        type = $(this).attr('type');
        required = $(this).attr('required');
        validation = $(this).attr('validation');
        valid = valid & EC.ValidateField(dtdid, id, type, required, validation);
    });
    return valid;
}

TheFarm.Umbraco.EmbeddedContent.ValidateField = function (dtdid, fieldId, type, required, validation) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    if (type == 'True/false') {
        //won't be validated! 
        return true;
    } else if (type == 'Content picker' || type == 'Media picker') {
        value = $("input[id$='field" + dtdid + "_" + fieldId + "_ContentIdValue']").val();
    } else if (type == 'Date picker') {
        value = $("td[id$='field" + dtdid + "_" + fieldId + "'] input").val();
    } else if (type == 'Textbox multiple' || type == 'Simple editor') {
        value = $("textarea[id$='field" + dtdid + "_" + fieldId + "']").val();
    } else {
        value = $("input[id$='field" + dtdid + "_" + fieldId + "']").val();
    }
    if (required == 'True' && value == '') {
        EC.MarkField(dtdid, fieldId, type);
        return false;
    }
    if (validation != '') {
        regex = new RegExp(validation);
        result = value.match(regex);
        if (!result || result == null || result == '') {
            EC.MarkField(dtdid, fieldId, type);
            return false;
        }
    }
    EC.UnmarkField(dtdid, fieldId, type);
    return true;
}

TheFarm.Umbraco.EmbeddedContent.MarkField = function (dtdid, fieldId, type) {
    if (type == 'Content picker' || type == 'Media picker') {
        $("td:has(input[id$='field" + dtdid + "_" + fieldId + "_ContentIdValue'])").prev().css('color', 'red').css('font-weight', 'bold');
    } else if (type == 'Date picker') {
        $("td[id$='field" + dtdid + "_" + fieldId + "']").prev().css('color', 'red').css('font-weight', 'bold');
    } else if (type == 'Textbox multiple' || type == 'Simple editor') {
        $("td:has(textarea[id$='field" + dtdid + "_" + fieldId + "'])").prev().css('color', 'red').css('font-weight', 'bold');
    } else {
        $("td:has(input[id$='field" + dtdid + "_" + fieldId + "'])").prev().css('color', 'red').css('font-weight', 'bold');
    }
}

TheFarm.Umbraco.EmbeddedContent.UnmarkField = function (dtdid, fieldId, type) {
    
    if (type == 'Content picker' || type == 'Media picker') {
        $("td:has(input[id$='field" + dtdid + "_" + fieldId + "_ContentIdValue'])").prev().css('color', 'black').css('font-weight', 'normal');
    } else if (type == 'Date picker') {
        $("td[id$='field" + dtdid + "_" + fieldId + "']").prev().css('color', 'black').css('font-weight', 'normal');
    } else if (type == 'Textbox multiple' || type == 'Simple editor') {
        $("td:has(textarea[id$='field" + dtdid + "_" + fieldId + "'])").prev().css('color', 'black').css('font-weight', 'normal');
    } else {
        $("td:has(input[id$='field" + dtdid + "_" + fieldId + "'])").prev().css('color', 'black').css('font-weight', 'normal');
    }
}

//used for changing the order after a successful dragsort
TheFarm.Umbraco.EmbeddedContent.WriteHiddenValueFromList = function (dtdid) {
    valuexml = decodeURIComponent($("input[id$='hiddenXmlValue" + dtdid + "']").val());
    dojo.require("dojox.xml.parser");
    xmlDoc = dojox.xml.parser.parse(valuexml);

    items = new Array();

    if (xmlDoc.documentElement && xmlDoc.documentElement.hasChildNodes()) {
        childNodes = xmlDoc.documentElement.childNodes;
        for (i = 0; i < childNodes.length; i++) {
            attributes = childNodes[i].attributes;
            oldId = 0;
            for (j = 0; j < attributes.length; j++) {
                if (attributes[j].name == 'id') {
                    oldId = attributes[j].value;
                }
            }
            items[items.length] = { id: oldId, properties: dojox.xml.parser.innerXML(childNodes[i]) };
        }
    }
    

    valuexml = '<data>';
    $('ul#sortList' + dtdid + ' li').each(function () {
        id = $(this).find('span').html();
        for (i = 0; i < items.length; i++) {
            if (items[i].id == id) {
                valuexml += items[i].properties;
            }
        }
    });
    valuexml += '</data>';
    $("input[id$='hiddenXmlValue" + dtdid + "']").val(encodeURIComponent(valuexml));
}

//removes an item from the value xml
TheFarm.Umbraco.EmbeddedContent.RemoveItem = function (elem) {
    //get dtdid
    ul = elem.parent().parent().parent();
    dtdid = ul.attr('id').replace(/.*sortList/i, '');
    EC = TheFarm.Umbraco.EmbeddedContent;
    //find out if an element was selected before; if so unselect and reselect if cancelled
    previousSelectedLI = null;
    $('ul#sortList' + dtdid + ' li').each(function () {
        if ($(this).hasClass('EChighlight')) {
            previousSelectedLI = $(this);
        }
    });
    EC.ResetLIs(dtdid);
    //if the addBox is open close it and re-open it when cancelled
    reOpenAddBox = false;
    if ($("div#addBox" + dtdid).css('display') != 'none') {
        $("div#addBox" + dtdid).hide();
        reOpenAddBox = true;
    }
    elem.parent().parent().removeClass('ECregular').addClass('EChighlight');
    ok = confirm('Are you sure you want to delete "' + elem.parent().html().replace(/<div (.){0,40}ECbigButton[^<]*<\/div>/gi, '').replace(/<div[^<]*<\/div>/gi, '').replace(/<a[^<]*<\/a>/gi, '') + '"?');
    if (ok) {
        //remove from xml value
        id = elem.parent().parent().find('span').html();
        valuexml = decodeURIComponent($("input[id$='hiddenXmlValue" + dtdid + "']").val());
        regex = new RegExp('<item id="' + id + '"([^<]|<[^\/]|<\/[^i]|<\/i[^t]|<\/it[^e]|<\/ite[^m]|<\/item[^>])*<\/item>');
        valuexml = valuexml.replace(regex, '');
        $("input[id$='hiddenXmlValue" + dtdid + "']").val(encodeURIComponent(valuexml));
        //make the graphical changes
        elem.parent().parent().remove();
        EC.ClearFields(dtdid);
        $("div#addBox" + dtdid).hide();
        $("a#addLink" + dtdid).show();
    } else {
        elem.parent().parent().removeClass('EChighlight').addClass('ECregular');
        if (reOpenAddBox) {
            $("div#addBox" + dtdid).show();
        };
        if (previousSelectedLI != null) {
            previousSelectedLI.addClass('EChighlight');
        }
    }
    return false;
}

//edits an item = opens the add box and fills in the values for that field
TheFarm.Umbraco.EmbeddedContent.EditItem = function (elem) {
    //get dtdid
    ul = elem.parent().parent().parent();
    dtdid = ul.attr('id').replace(/.*sortList/i, '');
    EC = TheFarm.Umbraco.EmbeddedContent;
    EC.ResetLIs(dtdid);
    elem.parent().parent().removeClass('ECregular').addClass('EChighlight');
    EC.ClearFields(dtdid);
    id = elem.parent().parent().find('span').html();
    $("input#hiddenEditId" + dtdid).val(id);
    valuexml = decodeURIComponent($("input[id$='hiddenXmlValue" + dtdid + "']").val());

    dojo.require("dojox.xml.parser");
    xmlDoc = dojox.xml.parser.parse(valuexml);
    if (xmlDoc.documentElement && xmlDoc.documentElement.hasChildNodes()) {
        childNodes = xmlDoc.documentElement.childNodes;
        for (i = 0; i < childNodes.length; i++) {
            childAttributes = childNodes[i].attributes;
            for (j = 0; j < childAttributes.length; j++) {
                if (childAttributes[j].name == 'id') {
                    oldId = childAttributes[j].value;
                }
            }
            if (oldId == id) {
                grandChildren = childNodes[i].childNodes;
                for (k = 0; k < grandChildren.length; k++) {
                    grandChildAttributes = grandChildren[k].attributes;
                    for (l = 0; l < grandChildAttributes.length; l++) {
                        if (grandChildAttributes[l].name == 'type') {
                            type = grandChildAttributes[l].value;
                        } else if (grandChildAttributes[l].name == 'propertyid') {
                            propertyid = grandChildAttributes[l].value;
                        }
                    }
                    if (type == 'Simple editor' || type == 'Textbox multiple' || type == 'Textstring') {
                        regex = new RegExp('<' + grandChildren[k].nodeName + '[^>]*>');
                        regex2 = new RegExp('<\/' + grandChildren[k].nodeName + '[^>]*>');
                        //value = grandChildren[k].xml.replace(regex, '').replace(regex2, '');
                        value = dojox.xml.parser.innerXML(grandChildren[k])
                            .replace(regex, '')
                            .replace(regex2, '')
                            .replace(/&lt;/gi, '<')
                            .replace(/&gt;/gi, '>')
                            .replace(/&amp;/gi, '&');
                        EC.FillField(dtdid, value, type, propertyid);
                    } else {
                        //EC.FillField(dtdid, grandChildren[k].text, type, propertyid);
                        EC.FillField(dtdid,  dojox.xml.parser.textContent(grandChildren[k]), type, propertyid);
                    }
                }
            }
        }
        //}
    }
    $("div#addBox" + dtdid).show();
    $("a#addLink" + dtdid).hide();
    $("a#addPropertyLink" + dtdid + " div.ECbigButton").css('background-position', '0px -160px');
    EC.SetFocusOnFirstElement(dtdid);
    return false;
}

TheFarm.Umbraco.EmbeddedContent.FillField = function (dtdid, value, type, propertyid) {
    if (type == 'True/false') {
        if (value == '1') {
            $("input[id$='field" + dtdid + "_" + propertyid + "']").attr('checked', 'checked');
        } else {
            $("input[id$='field" + dtdid + "_" + propertyid + "']").removeAttr('checked');
        }
    } else if (type == 'Content picker' || type == 'Media picker') {
        if (value != '') {
            $("input[id$='field" + dtdid + "_" + propertyid + "_ContentIdValue']").val(value);
            //get the name of the node 
            $("span[id$='field" + dtdid + "_" + propertyid + "_btns']").show();

            $.ajax({
                type: 'POST',
                url: '/umbraco/rest/umbraconodename?id=' + value,
                success: function (data) {
                    name = $(data).text();
                    if (name != '' && name != 'no node found') {
                        $("span[id$='field" + dtdid + "_" + propertyid + "_title']").html(name);
                    }
                },
                error: function (msg) {
                    $.ajax({
                        type: 'POST',
                        url: '/umbraco/webservices/UmbracoNodeNameService.asmx/ReadUmbracoNodeName',
                        data: '{qid:"' + value + '"}',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (msg) {
                            name = msg.d;
                            if (name != '' && name != 'no node found') {
                                $("span[id$='field" + dtdid + "_" + propertyid + "_title']").html(name);
                            }
                        }
                    });
                }
            });
        }
    } else if (type == 'Simple editor') {
        $("textarea[id$='field" + dtdid + "_" + propertyid + "']").val(value);
    } else if (type == 'Date picker') {
        $("td[id$='field" + dtdid + "_" + propertyid + "'] input").val(value);
    } else if (type == 'Textbox multiple') {
        $("textarea[id$='field" + dtdid + "_" + propertyid + "']").val(value);
    } else {
        $("input[id$='field" + dtdid + "_" + propertyid + "']").val(value);
    }
}

//Calculates the next avaiable Id to be used with a newly created property element
TheFarm.Umbraco.EmbeddedContent.CalculateNextAvailableId = function (dtdid) {
    currentMaxId = 0;
    $("ul#sortList" + dtdid + " li span").each(function () {
        thisNumber = parseInt($(this).html());
        if (thisNumber > currentMaxId) {
            currentMaxId = thisNumber;
        }
    });
    return (currentMaxId + 1);
}

TheFarm.Umbraco.EmbeddedContent.AddPropertyClicked = function (dtdid) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    if (!EC.ValidateFields(dtdid)) return false;

    editId = $("input#hiddenEditId" + dtdid).val();
    valuexml = decodeURIComponent($("input[id$='hiddenXmlValue" + dtdid + "']").val());
    schemaxml = decodeURIComponent($("input[id$='EChiddenXmlSchema" + dtdid + "']").val());
    schemaxml = schemaxml.replace(/id=""/, 'id="' + (editId != '' ? editId : EC.CalculateNextAvailableId(dtdid)) + '"');
    dojo.require("dojox.xml.parser");
    xmlDoc = dojox.xml.parser.parse(schemaxml);
    //xmlDocValue = dojox.xml.parser.parse(valuexml);

    //    if (window.DOMParser) {
    //        doc = schemaxml;
    //    } else // Internet Explorer" + Environment.NewLine +
    //    {
    //        xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
    //        xmlDoc.async = "false";
    //        xmlDoc.loadXML(schemaxml);
    //        doc = $(xmlDoc).children();
    //    }
    if (xmlDoc.documentElement && xmlDoc.documentElement.hasChildNodes()) {
        childNodes = xmlDoc.documentElement.childNodes;
        for (i = 0; i < childNodes.length; i++) {
            childAttributes = childNodes[i].attributes;
            value = '';
            for (j = 0; j < childAttributes.length; j++) {
                if (childAttributes[j].name == 'type') {
                    type = childAttributes[j].value;
                } else if (childAttributes[j].name == 'propertyid') {
                    id = childAttributes[j].value;
                }
            }
            if (type == 'True/false') {
                value = ($("input[id$='field" + dtdid + "_" + id + "']").is(':checked') ? '1' : '0');
            } else if (type == 'Content picker' || type == 'Media picker') {
                value = $("input[id$='field" + dtdid + "_" + id + "_ContentIdValue']").val();
            } else if (type == 'Date picker') {
                value = $("td[id$='field" + dtdid + "_" + id + "'] input").val();
            } else if (type == 'Textbox multiple' || type == 'Simple editor') {
                value = $("textarea[id$='field" + dtdid + "_" + id + "']").val();
            } else {
                value = $("input[id$='field" + dtdid + "_" + id + "']").val();
            }
            schemaxml = schemaxml.replace('|value' + id + '|', value.replace(/&/gi, '&amp;').replace(/</gi, '&lt;').replace(/>/gi, '&gt;'));
        }
    }
    //    $(doc).children().each(function () {
    //        id = $(this).attr('propertyid');
    //        //check for different types
    //        value = '';
    //        type = $(this).attr('type');
    //        if (type == 'True/false') {
    //            value = ($("input[id$='field" + dtdid + "_" + id + "']").is(':checked') ? '1' : '0');
    //        } else if (type == 'Content picker' || type == 'Media picker') {
    //            value = $("input[id$='field" + dtdid + "_" + id + "_ContentIdValue']").val();
    //        } else if (type == 'Date picker') {
    //            value = $("td[id$='field" + dtdid + "_" + id + "'] input").val();
    //        } else if (type == 'Textbox multiple' || type == 'Simple editor') {
    //            value = $("textarea[id$='field" + dtdid + "_" + id + "']").val();
    //        } else {
    //            value = $("input[id$='field" + dtdid + "_" + id + "']").val();
    //        }
    //        schemaxml = schemaxml.replace('|value' + id + '|', value.replace(/&/gi, '&amp;').replace(/</gi, '&lt;').replace(/>/gi, '&gt;'));
    //    });
    if (editId == '') {
        valuexml = valuexml.replace(/<\/data>/, schemaxml + '</data>');
    } else {
        regex = new RegExp('<item id="' + editId + '"([^<]|<[^\/]|<\/[^i]|<\/i[^t]|<\/it[^e]|<\/ite[^m]|<\/item[^>])*<\/item>');
        valuexml = valuexml.replace(regex, schemaxml);
    }
    $("input[id$='hiddenXmlValue" + dtdid + "']").val(encodeURIComponent(valuexml));
    EC.PopulateListFromHiddenValue(dtdid);
    EC.SetBigButtonBackground(false);
    EC.ClearFields(dtdid);
    $("div#addBox" + dtdid).hide();
    $("a#addLink" + dtdid).show();
    return false;
}

TheFarm.Umbraco.EmbeddedContent.InitializeDragSort = function(dtdid) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    $("ul#sortList" + dtdid).dragsort({
        dragEnd: function () { EC.WriteHiddenValueFromList(dtdid); },
        dragSelector: "div.ECdraghandle"
    });
}

TheFarm.Umbraco.EmbeddedContent.PopulateListFromHiddenValue = function(dtdid) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    $('ul#sortList' + dtdid).html('');
    //hiddenXmlValueId = 'EChiddenXmlValue' + dtdid;
    xml = decodeURIComponent($("input[id$='hiddenXmlValue" + dtdid + "']").val());
    if( window.DOMParser )
    {
        parser=new DOMParser();
        xmlDoc=parser.parseFromString(xml,"text/xml");
    }
    else // Internet Explorer
    {
        xmlDoc=new ActiveXObject("Microsoft.XMLDOM");
        xmlDoc.async = "false";
        xmlDoc.loadXML(xml);
    }
    $(xmlDoc).find('item').each(function(){
        li = EC.htmlLiStart +
            EC.htmlDragHandle +
            EC.htmlSpanStart +
            $(this).attr('id') +
            EC.htmlSpanEnd +
            EC.htmlTextDivStart;
        separator = '';
        $(this).children().each(function(){
            if ($(this).attr('showintitle') == 'True'){
                li += separator + $(this).text();  // + $(this) + ': '
                separator = ', ';
            }
        });
        li += EC.htmlEditButton +
            EC.htmlRemoveButton +
            EC.htmlTextDivEnd +
            EC.htmlLiEnd;
        $('ul#sortList' + dtdid).append(li);
    });
}

TheFarm.Umbraco.EmbeddedContent.ResetLIs = function(dtdid) {
    $('ul#sortList' + dtdid + ' li').each(function(){
        $(this).removeClass('EChighlight').removeClass('ECregular').addClass('ECregular');
    });
}

//Clears out the add box fields
TheFarm.Umbraco.EmbeddedContent.ClearFields = function (dtdid) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    $("input#hiddenEditId" + dtdid).val('');
    schemaxml = decodeURIComponent($("input[id$='EChiddenXmlSchema" + dtdid + "']").val());
    if (window.DOMParser) {
        $(schemaxml).children().each(function () {
            //check for different types
            EC.ClearField($(this).attr('propertyid'), $(this).attr('type'), dtdid);
            EC.UnmarkField(dtdid, $(this).attr('propertyid'), $(this).attr('type'));
        });
    }
    else // Internet Explorer
    {
        xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
        xmlDoc.async = "false";
        xmlDoc.loadXML(schemaxml);
        xmlDoc.onreadystatechange = function () { if (xmlDoc.readyState != 4) return false; };
        if (xmlDoc.documentElement && xmlDoc.documentElement.hasChildNodes()) {
            childNodes = xmlDoc.documentElement.childNodes;
            for (i = 0; i < childNodes.length; i++) {
                attributes = childNodes[i].attributes;
                for (j = 0; j < attributes.length; j++) {
                    if (attributes[j].name == 'type') {
                        type = attributes[j].value;
                    } else if (attributes[j].name == 'propertyid') {
                        id = attributes[j].value;
                    }
                }
                EC.ClearField(id, type, dtdid);
                EC.UnmarkField(dtdid, id, type);
            }
        }
    }
}

TheFarm.Umbraco.EmbeddedContent.ClearField = function (id, type, dtdid) {
    if (type == 'True/false') {
        $("input[id$='field" + dtdid + "_" + id + "']").removeAttr('checked');
    } else if (type == 'Content picker' || type == 'Media picker') {
        $("input[id$='field" + dtdid + "_" + id + "_ContentIdValue']").val('');
        $("span[id$='field" + dtdid + "_" + id + "_btns']").hide();
        $("span[id$='field" + dtdid + "_" + id + "_title']").html('');
    } else if (type == 'Date picker') {
        $("td[id$='field" + dtdid + "_" + id + "'] input").val('');
    } else if (type == 'Textbox multiple' || type == 'Simple editor') {
        $("textarea[id$='field" + dtdid + "_" + id + "']").val('');
    } else {
        $("input[id$='field" + dtdid + "_" + id + "']").val('');
    }
}

TheFarm.Umbraco.EmbeddedContent.AddLinkClicked = function (dtdid, addLink) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    if (!EC.CheckMaxCount(dtdid)) return false;
    EC.ResetLIs(dtdid);
    $("input#hiddenEditId" + dtdid).val('');
    $("div#addBox" + dtdid).show();
    addLink.hide();
    $("a#addPropertyLink" + dtdid + " div.ECbigButton").css('background-position', '0px -80px');
    EC.ClearFields(dtdid);
    EC.SetFocusOnFirstElement(dtdid);
    return false;
}

TheFarm.Umbraco.EmbeddedContent.CloseLinkClicked = function (dtdid) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    EC.ResetLIs(dtdid);
    EC.ClearFields(dtdid);
    $("div#addBox" + dtdid).hide();
    $("a#addLink" + dtdid).show();
    return false;
}

//Sets the focus on the first item when 'add' or 'edit' is clicked
TheFarm.Umbraco.EmbeddedContent.SetFocusOnFirstElement = function (dtdid) {
    schemaxml = decodeURIComponent($("input[id$='EChiddenXmlSchema" + dtdid + "']").val());
    if (window.DOMParser) {
        doc = schemaxml;
    } else // Internet Explorer" + Environment.NewLine +
    {
        xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
        xmlDoc.async = "false";
        xmlDoc.loadXML(schemaxml);
        doc = $(xmlDoc).children();
    }
    firstItem = $(doc).children(":first");
    if (firstItem) {
        id = firstItem.attr('propertyid');
        type = firstItem.attr('type');

        if (type == 'True/false') {
            $("input[id$='field" + dtdid + "_" + id + "']").focus();
        } else if (type == 'Content picker' || type == 'Media picker') {
            $("span[id$='field" + dtdid + "_" + id + "_btns']").next().focus();
        } else if (type == 'Simple editor') {
            $("textarea[id$='field" + dtdid + "_" + id + "']").focus().select();
        } else if (type == 'Date picker') {
            $("td[id$='field" + dtdid + "_" + id + "'] input").next().focus();
        } else if (type == 'Textbox multiple') {
            $("textarea[id$='field" + dtdid + "_" + id + "']").focus().select();
        } else {
            $("input[id$='field" + dtdid + "_" + id + "']").focus().select();
        }
    }
}
