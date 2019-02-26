
/*
依赖
config.js
*/

var switchDisplayModeNav = { 
    name: '切换模式', 
    link: 'javascript:void(0)', 
    onclick: function(index) { 
        if (app.switchDisplayMode != undefined) app.switchDisplayMode(); 
    } 
};