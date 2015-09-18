TheFarm.Umbraco.EmbeddedContent.ParseList = function () {
    returnValue = $("ul#sortList").html();
    returnValue = returnValue.replace(/\|/gi, '@@@');
    returnValue = returnValue.replace(/<li[^>]*>/gi, '||');
    returnValue = returnValue.replace(/<\/li[^>]*>/gi, '');
    returnValue = returnValue.replace(/<div [^>]*ECbigButton[^<]*<\/div>/gi, '');
    returnValue = returnValue.replace(/<a[^<]*<\/a>/gi, '');
    returnValue = returnValue.replace(/<div [^>]*ECdraghandle[^<]*<\/div>/gi, '');
    returnValue = returnValue.replace(/<span>(\d*)<\/span>Name:/gi, 'id:$1|Name:');
    returnValue = returnValue.replace(/<span>/gi, '');
    returnValue = returnValue.replace(/<\/span>/gi, '');
    returnValue = returnValue.replace(/<div [^>]*ECitemInner[^>]*>/gi, '');
    returnValue = returnValue.replace(/<\/div>/gi, '');
    returnValue = returnValue.replace(/\n/gi, '');
    returnValue = returnValue.replace(/\r/gi, '');
    returnValue = returnValue.replace(/; Alias:/gi, '| Alias:');
    returnValue = returnValue.replace(/; Type:/gi, '| Type:');
    returnValue = returnValue.replace(/; Description:/gi, '| Description:');
    returnValue = returnValue.replace(/; Show in title\?/gi, '| Show in title?');
    returnValue = returnValue.replace(/; Required\?/gi, '| Required?');
    returnValue = returnValue.replace(/; Validation:/gi, '| Validation:');
    return returnValue;
}

//SetHiddenValue stores the parsed property list in a hidden value so it can be read upon saving of the doc type
TheFarm.Umbraco.EmbeddedContent.SetHiddenValue = function() {
    EC = TheFarm.Umbraco.EmbeddedContent;
    $("input[id$='EChiddenValue']").val(EC.ParseList());
}

//Gets the value in the hidden input element for a specified id
TheFarm.Umbraco.EmbeddedContent.GetHiddenValue = function(id) {
    items = $("input[id$='EChiddenValue']").val().split('||');
    for (i = 1; i < items.length; i++){
        if (items[i].substr(0, (3 + id.length)) == ('id:' + id)){
            return items[i];
        }
    }
}

//Calculates the next avaiable Id to be used with a newly created property element
TheFarm.Umbraco.EmbeddedContent.CalculateNextAvailableId = function() {
    currentMaxId = 0;
    $("ul#sortList li span").each(function(){
        thisNumber = parseInt($(this).html());
        if (thisNumber > currentMaxId){
            currentMaxId = thisNumber;
        }
    });
    return (currentMaxId + 1);
}

//Clears out the add box fields
TheFarm.Umbraco.EmbeddedContent.ClearFields = function () {
    $("input#hiddenEditId").val('');
    $("input#name").val('');
    $("input#alias").val('');
    $("select#type").val('Textstring');
    $("textarea#description").val('');
    $("input#showInTitle").removeAttr('checked');
    $("input#required").removeAttr('checked');
    $("input#validation").val('');
    $("input[id$='name']").css('border', '1px solid #AAA');
    $("input[id$='alias']").css('border', '1px solid #AAA');
}

TheFarm.Umbraco.EmbeddedContent.RemoveItem = function(elem) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    //find out if an element was selected before; if so unselect and reselect if cancelled
    previousSelectedLI = null;
    $('ul#sortList li').each(function(){
        if ($(this).hasClass('EChighlight')){
            previousSelectedLI = $(this);
        }
    });
    EC.ResetLIs();
    //if the addBox is open close it and re-open it when cancelled
    reOpenAddBox = false;
    if ($("div#addBox").css('display') != 'none'){
        $("div#addBox").hide();
        reOpenAddBox = true;
    }
    elem.parent().parent().removeClass('ECregular').addClass('EChighlight');
    ok = confirm('Are you sure you want to delete \"' + elem.parent().html().replace(/<div (.){0,40}ECbigButton[^<]*<\/div>/gi, '').replace(/<div[^<]*<\/div>/gi, '').replace(/<a[^<]*<\/a>/gi, '').replace(/<span[^<]*<\/span>/gi, '') + '\"?');
    if (ok){
        elem.parent().parent().remove(); 
        EC.SetHiddenValue(); 
        EC.ClearFields(); 
        EC.HideAddBox();
    }else{ 
        elem.parent().parent().removeClass('EChighlight').addClass('ECregular');
        if (reOpenAddBox){
            $("div#addBox").show();
        };
        if (previousSelectedLI != null){
            previousSelectedLI.addClass('EChighlight');
        }
    } 
    return false;
}

TheFarm.Umbraco.EmbeddedContent.ResetLIs = function() {
    $('ul#sortList li').each(function(){
        $(this).removeClass('EChighlight').removeClass('ECregular').addClass('ECregular');
    });
}

//hides the add box form and shows the add link
TheFarm.Umbraco.EmbeddedContent.HideAddBox = function() {
    $("div#addBox").hide();
    $("a#addLink").show();
}

TheFarm.Umbraco.EmbeddedContent.EditProperty = function (elem) {
    EC = TheFarm.Umbraco.EmbeddedContent;
    EC.ClearFields();
    EC.ResetLIs();
    elem.parent().removeClass('ECregular').addClass('EChighlight');
    id = elem.find('span:first').html();
    $("input#hiddenEditId").val(id);
    $("div#addBox").show();
    $("a#addLink").hide();
    $("a#addPropertyLink div.ECbigButton").css('background-position', '0px -160px');
    properties = EC.GetHiddenValue(id).split('|');
    for (i = 1; i < properties.length; i++) {
        property = properties[i].replace(/^\s+|\s+$/g, '');
        if (property.substr(0, 4) == 'Name') {
            $("input#name").val(property.substr(5).replace(/^\s+|\s+$/g, ''));
        } else if (property.substr(0, 5) == 'Alias') {
            $("input#alias").val(property.substr(6).replace(/^\s+|\s+$/g, ''));
        } else if (property.substr(0, 4) == 'Type') {
            $("select#type").val(property.substr(5).replace(/^\s+|\s+$/g, ''));
        } else if (property.substr(0, 11) == 'Description') {
            $("textarea#description").val(property.substr(12).replace(/^\s+|\s+$/g, '').replace(/&lt;/gi, '<').replace(/&gt;/gi, '>'));
        } else if (property.substr(0, 14) == 'Show in title?') {
            if (property.substr(14).replace(/^\s+|\s+$/g, '') == 'true') {
                $("input#showInTitle").attr('checked', 'checked');
            } else {
                $("input#showInTitle").removeAttr('checked');
            }
        } else if (property.substr(0, 9) == 'Required?') {
            if (property.substr(9).replace(/^\s+|\s+$/g, '') == 'true') {
                $("input#required").attr('checked', 'checked');
            } else {
                $("input#required").removeAttr('checked');
            }
        } else if (property.substr(0, 10) == 'Validation') {
            $("input#validation").val(property.substr(11).replace(/^\s+|\s+$/g, '').replace(/@@@/gi, '|'));
        }
    }
    EC.SetFocusOnNameField();
}

TheFarm.Umbraco.EmbeddedContent.ToggleHelp = function () {
    //set helptext left align:
    $('#helpText').css('left', ($('#sortListContainer').position().left + 10) + 'px');
    $('#helpText').toggle();
    div = $('a.EChelplink div.backgroundSpritePrevalue');
    EC.SwitchButtonState(div);
    return false;
}

TheFarm.Umbraco.EmbeddedContent.ToggleType = function () {
    EC = TheFarm.Umbraco.EmbeddedContent;

    div = $('a.ECshowType div.backgroundSpritePrevalue');
    EC.SwitchButtonState(div);
    if (div.hasClass('off')) {
        EC.SetSetting('showType', '0');
    } else {
        EC.SetSetting('showType', '1');
    }
    EC.ApplySettings();
    return false;
}

TheFarm.Umbraco.EmbeddedContent.ToggleDescription = function () {
    EC = TheFarm.Umbraco.EmbeddedContent;

    div = $('a.ECshowDescription div.backgroundSpritePrevalue');
    EC.SwitchButtonState(div);
    if (div.hasClass('off')) {
        EC.SetSetting('showDescription', '0');
    } else {
        EC.SetSetting('showDescription', '1');
    }
    EC.ApplySettings();
    return false;
}

TheFarm.Umbraco.EmbeddedContent.ToggleTitle = function () {
    EC = TheFarm.Umbraco.EmbeddedContent;

    div = $('a.ECshowTitle div.backgroundSpritePrevalue');
    EC.SwitchButtonState(div);
    if (div.hasClass('off')) {
        EC.SetSetting('showTitle', '0');
    } else {
        EC.SetSetting('showTitle', '1');
    }
    EC.ApplySettings();
    return false;
}

TheFarm.Umbraco.EmbeddedContent.ToggleRequired = function () {
    EC = TheFarm.Umbraco.EmbeddedContent;

    div = $('a.ECshowRequired div.backgroundSpritePrevalue');
    EC.SwitchButtonState(div);
    if (div.hasClass('off')) {
        EC.SetSetting('showRequired', '0');
    } else {
        EC.SetSetting('showRequired', '1');
    }
    EC.ApplySettings();
    return false;
}

TheFarm.Umbraco.EmbeddedContent.ToggleValidation = function () {
    EC = TheFarm.Umbraco.EmbeddedContent;

    div = $('a.ECshowValidation div.backgroundSpritePrevalue');
    EC.SwitchButtonState(div);
    if (div.hasClass('off')) {
        EC.SetSetting('showValidation', '0');
    } else {
        EC.SetSetting('showValidation', '1');
    }
    EC.ApplySettings();
    return false;
}

TheFarm.Umbraco.EmbeddedContent.SwitchButtonState = function (div) {
    if (div) {
        if (div.hasClass('off')) {
            div.removeClass('off').addClass('on');
            y = null;
            if (div.css('background-position') != undefined) {
                position = div.css('background-position');
                if (position != undefined) {
                    positionXY = position.split(' ');
                    y = parseInt(positionXY[1].replace('px', ''));
                }
            } else if (div.css('background-position-y') != undefined) {
                y = parseInt(div.css('background-position-y').replace('px', ''));
            }
            if (y != null) {
                div.css('background-position', '0px ' + (y - 40) + 'px');
            }
        } else {
            div.removeClass('on').addClass('off');
            y = null;
            if (div.css('background-position') != undefined) {
                position = div.css('background-position');
                if (position != undefined) {
                    positionXY = position.split(' ');
                    y = parseInt(positionXY[1].replace('px', ''));
                }
            } else if (div.css('background-position-y') != undefined) {
                y = parseInt(div.css('background-position-y').replace('px', ''));
            }
            if (y != null) {
                div.css('background-position', '0px ' + (y + 40) + 'px');
            }
        }
    }
}

TheFarm.Umbraco.EmbeddedContent.SetBigButtonBackground = function (initHover) {
    if (TheFarm.Umbraco.EmbeddedContent.BigButtonsUrl) {
        $('.ECbigButton').each(function () {
            $(this).css('background-image', 'url("' + TheFarm.Umbraco.EmbeddedContent.BigButtonsUrl + '")');
            if (!initHover) {
                $(this).unbind('mouseover').unbind('mouseout');
            }
            $(this).bind('mouseover', function () {
                if ($(this) != undefined) {
                    if ($(this).css('background-position') != undefined) {
                        positionXY = $(this).css('background-position').split(' ');
                        if (positionXY != undefined) {
                            y = parseInt(positionXY[1].replace('px', ''));
                        }
                    } else {
                        y = parseInt($(this).css('background-position-y').replace('px', ''));
                    }
                    $(this).css('background-position', '0px ' + (y - 20) + 'px');
                }
            });
            $(this).bind('mouseout', function () {
                if ($(this) != undefined) {
                    if ($(this).css('background-position') != undefined) {
                        positionXY = $(this).css('background-position').split(' ');
                        if (positionXY != undefined) {
                            y = parseInt(positionXY[1].replace('px', ''));
                        }
                    } else {
                        y = parseInt($(this).css('background-position-y').replace('px', ''));
                    }
                    $(this).css('background-position', '0px ' + (y + 20) + 'px');
                }
            });
        });
    }
}

//sets a settings value after the user has changed it
TheFarm.Umbraco.EmbeddedContent.SetSetting = function (field, value) {
    settings = $("input[id$='EChiddenSettings']").val();
    regEx = new RegExp(field + '=\\d');
    newSettings = settings.replace(regEx, field + '=' + value);
    $("input[id$='EChiddenSettings']").val(newSettings);
}

//shows/hides fields in the list based on user settings
TheFarm.Umbraco.EmbeddedContent.ApplySettings = function () {
    settings = $("input[id$='EChiddenSettings']").val();
    if (settings == undefined) return false;

    setting = settings.split(',');
    for (i = 0; i < setting.length; i++) {
        s = setting[i].split('=');
        if (s[0] == 'showType') {
            $("ul#sortList li div.ECitemInner").each(function () {
                val = $(this).html();
                if (s[1] == '0') {
                    newVal = val.replace(/; Type: ([^;]*);(<span>){0,1} Description:/i, ';<span> Type: $1;</span>$2 Description:');
                } else {
                    newVal = val.replace(/;<span> Type: ([^;]*);<\/span>(<span>){0,1} Description:/i, '; Type: $1;$2 Description:');
                }
                $(this).html(newVal);
            });
        } else if (s[0] == 'showDescription') {
            $("ul#sortList li div.ECitemInner").each(function () {
                val = $(this).html();
                if (s[1] == '0') {
                    newVal = val.replace(/;(<\/span>){0,1} Description: (.*);(<span>){0,1} Show in title\?/i, ';$1<span> Description: $2;</span>$3 Show in title?');
                } else {
                    newVal = val.replace(/;(<\/span>){0,1}<span> Description: (.*);<\/span>(<span>){0,1} Show in title\?/i, ';$1 Description: $2;$3 Show in title?');
                }
                $(this).html(newVal);
            });
        } else if (s[0] == 'showTitle') {
            $("ul#sortList li div.ECitemInner").each(function () {
                val = $(this).html();
                if (s[1] == '0') {
                    newVal = val.replace(/;(<\/span>){0,1} Show in title\? ([^;]*);(<span>){0,1} Required\?/i, ';$1<span> Show in title? $2;</span>$3 Required?');
                } else {
                    newVal = val.replace(/;(<\/span>){0,1}<span> Show in title\? ([^;]*);<\/span>(<span>){0,1} Required\?/i, ';$1 Show in title? $2;$3 Required?');
                }
                $(this).html(newVal);
            });
        } else if (s[0] == 'showRequired') {
            $("ul#sortList li div.ECitemInner").each(function () {
                val = $(this).html();
                if (s[1] == '0') {
                    newVal = val.replace(/;(<\/span>){0,1} Required\? ([^;]*);(<span>){0,1} Validation:/i, ';$1<span> Required? $2;</span>$3 Validation:');
                } else {
                    newVal = val.replace(/;(<\/span>){0,1}<span> Required\? ([^;]*);<\/span>(<span>){0,1} Validation:/i, ';$1 Required? $2;$3 Validation:');
                }
                $(this).html(newVal);
            });
        } else if (s[0] == 'showValidation') {
            $("ul#sortList li div.ECitemInner").each(function () {
                val = $(this).html();
                if (s[1] == '0') {
                    newVal = val.replace(/;(<\/span>){0,1} Validation: ([^<]*)<a/i, ';$1<span> Validation: $2</span><a');
                } else {
                    newVal = val.replace(/;(<\/span>){0,1}<span> Validation: ([^<]*)<\/span><a/i, ';$1 Validation: $2<a');
                }
                $(this).html(newVal);
            });
        }
    }

    //need to get the hover states working again!
    EC.SetBigButtonBackground(false);
}

//shows/hides fields in the list based on user settings
TheFarm.Umbraco.EmbeddedContent.InitializeSettings = function () {
    EC = TheFarm.Umbraco.EmbeddedContent;
    settings = $("input[id$='EChiddenSettings']").val();
    if (settings == '' || settings == undefined)
        settings = 'showType=1,showDescription=0,showTitle=0,showRequired=1,showValidation=0';
    setting = settings.split(',');
    for (i = 0; i < setting.length; i++) {
        s = setting[i].split('=');
        if (s[1] == '1') {
            div = $('a.EC' + s[0] + ' div.backgroundSpritePrevalue');
            EC.SwitchButtonState(div);
        }
    }
}

TheFarm.Umbraco.EmbeddedContent.ValidatePrevalueFields = function () {
    EC = TheFarm.Umbraco.EmbeddedContent;
    return EC.ValidatePrevalueName() & EC.ValidatePrevalueAlias();
}

TheFarm.Umbraco.EmbeddedContent.ValidatePrevalueName = function () {
    field = $("input[id$='name']");
    if (field.val() == '') {
        field.css('border', '2px solid red');
        return false;
    } else {
        field.css('border', '1px solid #AAA');
        return true;
    }
}

TheFarm.Umbraco.EmbeddedContent.ValidatePrevalueAlias = function () {
    field = $("input[id$='alias']");
    if (field.val() == '') {
        field.css('border', '2px solid red');
        return false;
    } else {
        field.css('border', '1px solid #AAA');
        return true;
    }
}

//sets the focus on the name field if 'add' or 'edit' has been clicked
TheFarm.Umbraco.EmbeddedContent.SetFocusOnNameField = function () {
    $("input#name").focus().select();
}

$(document).ready(function () {
    EC = TheFarm.Umbraco.EmbeddedContent;

    if (EC.FarmLogoUrl) {
        //initializes the jquery.dragsort functionality
        $("ul#sortList").dragsort({
            dragEnd: function () { EC.SetHiddenValue(); },
            dragSelector: "div.ECdraghandle"
        });

        //hooks up the click event to the addPropertyLink button which adds a new property to the list
        //the new element is generated by using the values from the fields of the add form box
        //the element is then added to the inner html of the list
        $("a#addPropertyLink").click(function () {
            if (!EC.ValidatePrevalueFields()) return false; ;

            //if the hidden id is other than 0 delete the existing entry first
            id = $("input#hiddenEditId").val();
            if (id != '') {
                li = $("ul#sortList li:has(span:contains('" + id + "'))");
            } else {
                li = $("ul#sortList li:last");
            }
            newElement = EC.HtmlSpanStart + (id != '' ? id : EC.CalculateNextAvailableId()) + EC.HtmlSpanEnd +
                'Name: ' + $("input#name").val() +
                '; Alias: ' + $("input#alias").val() +
                '; Type: ' + $("select#type").val() +
                '; Description: ' + $("textarea#description").val().replace(/"/gi, '&quot').replace(/</gi, '&lt;').replace(/>/gi, '&gt;') +
                '; Show in title? ' + $("input#showInTitle").attr('checked') +
                '; Required? ' + $("input#required").attr('checked') +
                '; Validation: ' + $("input#validation").val();
            if (li.size() != 0) {
                li.after(EC.HtmlLiStart + EC.HtmlDragHandle + EC.HtmlTextDivStart + newElement + EC.HtmlEditButton + EC.HtmlRemoveButton + EC.HtmlTextDivEnd + EC.HtmlLiEnd);
                if (id != '') {
                    li.remove();
                }
            } else {
                //the list is empty" + Environment.NewLine +
                $("ul#sortList").append(EC.HtmlLiStart + EC.HtmlDragHandle + EC.HtmlTextDivStart + newElement + EC.HtmlEditButton + EC.HtmlRemoveButton + EC.HtmlTextDivEnd + EC.HtmlLiEnd);
            }
            EC.ApplySettings();
            EC.SetBigButtonBackground(false);
            EC.SetHiddenValue();
            EC.ClearFields();
            $("div#addBox").hide();
            $("a#addLink").show();
            return false;
        });

        //view function: opens the add form box
        $("a#addLink").click(function () {
            EC.ResetLIs();
            $("div#addBox").show();
            $(this).hide();
            $("a#addPropertyLink div.ECbigButton").css('background-position', '0px -80px');
            EC.ClearFields();
            EC.SetFocusOnNameField();
            return false;
        });

        //view function: closes the add form box
        $("a#closeLink").click(function () {
            EC.ResetLIs();
            EC.ClearFields();
            $("div#addBox").hide();
            $("a#addLink").show();
            return false;
        });

        //TheFARM logo and link
        $('#addLink').after('<br />');
        $('#addLink').after('<a href="http://www.thefarmdigital.com.au" title="Grown on TheFARM" class="farm" target="_blank"><span>Grown on TheFARM</span></a>');
        $('.farm').css('background-image', 'url("' + EC.FarmLogoUrl + '")');
        $('.farm').css('width', '112px');
        $('.farm').css('height', '17px');
        $('.farm').css('float', 'right');
        $('a.farm').html('');

        if (TheFarm.Umbraco.EmbeddedContent.BackgroundSpriteUrl) {
            $('.backgroundSpritePrevalue').each(function () {
                $(this).css('background-image', 'url("' + TheFarm.Umbraco.EmbeddedContent.BackgroundSpriteUrl + '")');
                $(this).hover(
                    function () {
                        if ($(this) != undefined) {
                            if ($(this).css('background-position') != undefined) {
                                positionXY = $(this).css('background-position').split(' ');
                                if (positionXY != undefined) {
                                    y = parseInt(positionXY[1].replace('px', ''));
                                }
                            } else {
                                y = parseInt($(this).css('background-position-y').replace('px', ''));
                            }
                            $(this).css('background-position', '0px ' + (y - 20) + 'px');
                        }
                    },
                    function () {
                        if ($(this) != undefined) {
                            if ($(this).css('background-position') != undefined) {
                                positionXY = $(this).css('background-position').split(' ');
                                if (positionXY != undefined) {
                                    y = parseInt(positionXY[1].replace('px', ''));
                                }
                            } else {
                                y = parseInt($(this).css('background-position-y').replace('px', ''));
                            }
                            $(this).css('background-position', '0px ' + (y + 20) + 'px');
                        }
                    }
                );
            });
        }
    }

    $("input[id$='name']").blur(function () {
        EC.ValidatePrevalueName();
    });
    $("input[id$='alias']").blur(function () {
        EC.ValidatePrevalueAlias();
    });

});

$(window).load(function () {
    EC.InitializeSettings();
    EC.ApplySettings();

});
