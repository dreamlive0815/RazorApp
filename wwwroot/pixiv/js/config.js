/*
配置信息
*/

var config = {
    apiUrlPrefix: '/pixiv/',
    pageUrlPrefix: './',
    defaultAutoSlideInterval: 5000,
    defaultHeadText: 'Pixiv',
    defaultNavs: [
        { name: '首页', link: './index.html' },
    ],

    getLoginApiUrl: function() {
        return this.apiUrlPrefix + 'login';
    },

    getIndexPageUrl: function() {
        return this.pageUrlPrefix + 'index.html';
    },
    getLoginPageUrl: function() {
        return this.pageUrlPrefix + 'login.html';
    }
};