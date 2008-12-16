﻿/**
 * @version 1.0.3
 * The MIT License
 * Copyright (c) 2008 Fabio M. Costa http://www.meiocodigo.com
 */
(function(B){var A=(window.orientation!=undefined);B.extend({mask:{rules:{"z":/[a-z]/,"Z":/[A-Z]/,"a":/[a-zA-Z]/,"*":/[0-9a-zA-Z]/,"@":/[0-9a-zA-ZÃ§Ã‡Ã¡Ã Ã£Ã©Ã¨Ã­Ã¬Ã³Ã²ÃµÃºÃ¹Ã¼]/},fixedChars:"[(),.:/ -]",ignoreKeys:[8,9,13,16,17,18,33,34,35,36,37,38,39,40,45,46,91,116],iphoneIgnoreKeys:[10,127],signals:["+","-"],options:{attr:"alt",mask:null,type:"fixed",defaultValue:"",signal:false,onInvalid:function(){},onValid:function(){},onOverflow:function(){}},masks:{"phone":{mask:"(99) 9999-9999"},"phone-us":{mask:"(999) 9999-9999"},"cpf":{mask:"999.999.999-99"},"cnpj":{mask:"99.999.999/9999-99"},"date":{mask:"39/19/9999"},"date-us":{mask:"19/39/9999"},"cep":{mask:"99999-999"},"time":{mask:"29:69"},"cc":{mask:"9999 9999 9999 9999"},"integer":{mask:"999.999.999.999",type:"reverse"},"decimal":{mask:"99,999.999.999.999",type:"reverse",defaultValue:"000"},"decimal-us":{mask:"99.999,999,999,999",type:"reverse",defaultValue:"000"},"signed-decimal":{mask:"99,999.999.999.999",type:"reverse",defaultValue:"+000"},"signed-decimal-us":{mask:"99,999.999.999.999",type:"reverse",defaultValue:"+000"}},MAXLENGTH:{ie:2147483647},init:function(){if(!this.hasInit){var C;this.ignore=false;this.fixedCharsReg=new RegExp(this.fixedChars);this.fixedCharsRegG=new RegExp(this.fixedChars,"g");for(C=0;C<=9;C++){this.rules[C]=new RegExp("[0-"+C+"]")}this.hasInit=true}},set:function(G,D){var C=this,E=B(G),F="maxlength";this.init();return E.each(function(){var N=B(this),O=B.extend({},C.options),M=N.attr(O.attr),H="",J=C.__getPasteEvent();H=(typeof D=="string")?D:(M!="")?M:null;if(H){O.mask=H}if(C.masks[H]){O=B.extend(O,C.masks[H])}if(typeof D=="object"){O=B.extend(O,D)}if(B.metadata){O=B.extend(O,N.metadata())}if(O.mask!=null){if(N.data("mask")){C.unset(N)}var I=O.defaultValue,L=N.attr(F),K=(O.type=="reverse");O=B.extend({},O,{maxlength:L,maskArray:O.mask.split(""),maskNonFixedCharsArray:O.mask.replace(C.fixedCharsRegG,"").split(""),defaultValue:I.split("")});if(K){N.css("text-align","right")}if(N.val()!=""){N.val(C.string(N.val(),O))}else{if(I!=""){N.val(C.string(I,O))}}N.data("mask",O);switch(true){case (B.browser.msie):N.attr(F,C.MAXLENGTH.ie);break;case (B.browser.safari):N.removeAttr(F);break;default:if(L>-1){N.removeAttr(F)}break}N.bind("keydown",{func:C._keyDown,thisObj:C},C._onMask).bind("keyup",{func:C._keyUp,thisObj:C},C._onMask).bind("keypress",{func:C._keyPress,thisObj:C},C._onMask).bind(J,{func:C._paste,thisObj:C},C._delayedOnMask)}})},unset:function(D){var C=B(D),E=this;return C.each(function(){var H=B(this);if(H.data("mask")){var F=H.data("mask").maxlength,G=E.__getPasteEvent();if(F==-1){H.removeAttr("maxlength")}else{H.attr("maxlength",F)}H.unbind("keydown",E._onMask).unbind("keypress",E._onMask).unbind("keyup",E._onMask).unbind(G,E._delayedOnMask).removeData("mask")}})},string:function(F,D){this.init();var E={};if(typeof F!="string"){F=String(F)}switch(typeof D){case"string":if(this.masks[D]){E=B.extend(E,this.masks[D])}else{E.mask=D}break;case"object":E=D;break}var C=(E.type=="reverse");this._insertSignal(C,F,E,this.signals);return this.__maskArray(F.split(""),E.mask.replace(this.fixedCharsRegG,"").split(""),E.mask.split(""),C,E.defaultValue,E.signal)},_onMask:function(C){var E=C.data.thisObj,D={};D._this=C.target;D.$this=B(D._this);if(D.$this.attr("readonly")){return true}D.value=D.$this.val();D.nKey=E.__getKeyNumber(C);D.range=E.__getRangePosition(D._this);D.valueArray=D.value.split("");D.data=D.$this.data("mask");D.reverse=(D.data.type=="reverse");return C.data.func.call(E,C,D)},_delayedOnMask:function(C){C.type="paste";setTimeout(function(){C.data.thisObj._onMask(C)},1)},_keyDown:function(C,D){if(A){this.ignore=(B.inArray(D.nKey,this.iphoneIgnoreKeys)>-1);return this._keyPress(C,D)}else{this.ignore=(B.inArray(D.nKey,this.ignoreKeys)>-1);return true}},_keyUp:function(C,D){if(D.nKey==9&&(B.browser.safari||B.browser.msie)){return true}return this._paste(C,D)},_paste:function(D,E){this._changeSignal(D.type,E);var C=this.__maskArray(E.valueArray,E.data.maskNonFixedCharsArray,E.data.maskArray,E.reverse,E.data.defaultValue,E.data.signal);E.$this.val(C);if(!E.reverse){this.__setRange(E._this,E.range.start,E.range.end)}return true},_keyPress:function(J,C){if(this.ignore||J.ctrlKey||J.metaKey||J.altKey){C.data.onValid.call(C._this,"",C.nKey);return true}this._changeSignal(J.type,C);var K=String.fromCharCode(C.nKey),M=C.range.start,G=C.value,E=C.data.maskArray;if(C.reverse){var F=G.substr(0,M),I=G.substr(C.range.end,G.length);G=(F+K+I);if(C.data.signal){M-=C.data.signal.length}}var L=G.replace(this.fixedCharsRegG,"").split(""),D=this.__extraPositionsTill(M,E);C.rsEp=M+D;if(!this.rules[E[C.rsEp]]){C.data.onOverflow.call(C._this,K,C.nKey);return false}else{if(!this.rules[E[C.rsEp]].test(K)){C.data.onInvalid.call(C._this,K,C.nKey);return false}else{C.data.onValid.call(C._this,K,C.nKey)}}var H=this.__maskArray(L,C.data.maskNonFixedCharsArray,E,C.reverse,C.data.defaultValue,C.data.signal,D);C.$this.val(H);return(C.reverse)?this._keyPressReverse(J,C):this._keyPressFixed(J,C)},_keyPressFixed:function(C,D){if(D.rangeStart==D.range.end||D.data.defaultValue){if((D.rsEp==0&&D.value.length==0)||D.rsEp<D.value.length){this.__setRange(D._this,D.rsEp,D.rsEp+1)}}else{this.__setRange(D._this,D.rangeStart,D.range.end)}return true},_keyPressReverse:function(C,D){if(B.browser.msie&&((D.rangeStart==0&&D.range.end==0)||D.rangeStart!=D.range.end)){this.__setRange(D._this,D.value.length)}return false},_setMaskData:function(F,C,E){var D=F.data("mask");D[C]=E;F.data("mask",D)},_changeSignal:function(D,E){if(E.data.signal!==false){var C=(D=="paste")?E.value.substr(0,1):String.fromCharCode(E.nKey);if(B.inArray(C,this.signals)>-1){if(C=="+"){C=""}this._setMaskData(E.$this,"signal",C);E.data.signal=C}}},_insertSignal:function(D,G,F,C){if(D&&F.defaultValue){if(typeof F.defaultValue=="string"){F.defaultValue=F.defaultValue.split("")}if(B.inArray(F.defaultValue[0],C)>-1){var E=G.substr(0,1);F.signal=(B.inArray(E,C)>-1)?E:F.defaultValue[0];F.defaultValue.shift()}}},__getPasteEvent:function(){return(B.browser.opera||(B.browser.mozilla&&parseFloat(B.browser.version.substr(0,3))<1.9))?"input":"paste"},__getKeyNumber:function(C){return(C.charCode||C.keyCode||C.which)},__maskArray:function(H,G,E,D,C,I,F){if(D){H.reverse()}H=this.__removeInvalidChars(H,G);if(C){H=this.__applyDefaultValue.call(H,C)}H=this.__applyMask(H,E,F);if(D){H.reverse();if(!I||I=="+"){I=""}return I+H.join("").substring(H.length-E.length)}else{return H.join("").substring(0,E.length)}},__applyDefaultValue:function(E){var C=E.length,D=this.length,F;for(F=D-1;F>=0;F--){if(this[F]==E[0]){this.pop()}else{break}}for(F=0;F<C;F++){if(!this[F]){this[F]=E[F]}}return this},__removeInvalidChars:function(E,D){for(var C=0;C<E.length;C++){if(D[C]&&this.rules[D[C]]&&!this.rules[D[C]].test(E[C])){E.splice(C,1);C--}}return E},__applyMask:function(E,C,F){if(typeof F=="undefined"){F=0}for(var D=0;D<E.length+F;D++){if(C[D]&&this.fixedCharsReg.test(C[D])){E.splice(D,0,C[D])}}return E},__extraPositionsTill:function(E,C){var D=0;while(this.fixedCharsReg.test(C[E])){E++;D++}return D},__setRange:function(E,F,C){if(typeof C=="undefined"){C=F}if(E.setSelectionRange){E.setSelectionRange(F,C)}else{var D=E.createTextRange();D.collapse();D.moveStart("character",F);D.moveEnd("character",C-F);D.select()}},__getRangePosition:function(D){if(!B.browser.msie){return{start:D.selectionStart,end:D.selectionEnd}}var E={start:0,end:0},C=document.selection.createRange();if(!C||C.parentElement()!=D){return E}E.start=0-C.duplicate().moveStart("character",-100000);E.end=E.start+C.text.length;return E}}});B.fn.extend({setMask:function(C){return B.mask.set(this,C)},unsetMask:function(){return B.mask.unset(this)}})})(jQuery)