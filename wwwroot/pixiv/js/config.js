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

    getBookmarkNewIllustsApiUrl: function() {
        return this.apiUrlPrefix + 'bookmarknewillusts';
    },
    getIllustInfoApiUrl: function() {
        return this.apiUrlPrefix + 'illust';
    },
    getLoginApiUrl: function() {
        return this.apiUrlPrefix + 'login';
    },
    getRankApiUrl: function() {
        return this.apiUrlPrefix + 'ranklist';
    },

    getBookmarkNewIllustsPageUrl: function() {
        return this.pageUrlPrefix + 'bookmarknewillusts.html';
    },
    getIllustPageUrl: function(illustId) {
        return this.pageUrlPrefix + 'illust.html?id=' + illustId;
    },
    getIndexPageUrl: function() {
        return this.pageUrlPrefix + 'index.html';
    },
    getLoginPageUrl: function() {
        return this.pageUrlPrefix + 'login.html';
    },
    getRankPageUrl: function() {
        return this.pageUrlPrefix + 'ranklist.html';
    }
};

config.defaultNavs.push({
    name: '关注用户的新作品',
    link: config.getBookmarkNewIllustsPageUrl(),
});

config.defaultNavs.push({
    name: '排行榜',
    link: config.getRankPageUrl(),
});