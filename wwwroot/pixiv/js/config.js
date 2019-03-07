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
    defaultCommentsPerPageCount: 10,

    getCommentsApiUrl: function() {
        return this.apiUrlPrefix + 'comments';
    },
    getBookmarkIllustsApiUrl: function() {
        return this.apiUrlPrefix + 'bookmarkillusts';
    },
    getBookmarkNewIllustsApiUrl: function() {
        return this.apiUrlPrefix + 'bookmarknewillusts';
    },
    getIllustInfoApiUrl: function() {
        return this.apiUrlPrefix + 'illust';
    },
    getLikeIllustUrl: function() {
        return this.apiUrlPrefix + 'likeillust';
    },
    getLoginApiUrl: function() {
        return this.apiUrlPrefix + 'login';
    },
    getMyFollowApiUrl: function() {
        return this.apiUrlPrefix + 'myfollow';
    },
    getRankApiUrl: function() {
        return this.apiUrlPrefix + 'ranklist';
    },
    getUserIllustsApiUrl: function() {
        return this.apiUrlPrefix + 'userillusts';
    },
    getUserProfileApiUrl: function() {
        return this.apiUrlPrefix + 'userprofile';
    },
    getFollowUserApiUrl: function() {
        return this.apiUrlPrefix + 'followuser';
    },
    getUnFollowUserApiUrl: function() {
        return this.apiUrlPrefix + 'unfollowuser';
    },
    getBookmarkIllustApiUrl: function() {
        return this.apiUrlPrefix + 'bookmarkillust';
    },
    getUnBookmarkIllustApiUrl: function() {
        return this.apiUrlPrefix + 'unbookmarkillust';
    },

    getBookmarkIllustsPageUrl: function() {
        return this.pageUrlPrefix + 'bookmarkillusts.html';
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
    getMyFollowPageUrl: function() {
        return this.pageUrlPrefix + 'myfollow.html';
    },
    getRankPageUrl: function() {
        return this.pageUrlPrefix + 'ranklist.html';
    },
    getUserIllustsPageUrl: function(userId) {
        return this.pageUrlPrefix + 'userillusts.html?id=' + userId;
    },

    getEmojiUrlById: function(emojiId) {
        return "https://s.pximg.net/common/images/stamp/generated-stamps/" + emojiId + "_s.jpg";
    }
};

config.defaultNavs.push({
    name: '关注的用户',
    link: config.getMyFollowPageUrl(),
});

config.defaultNavs.push({
    name: '关注用户的新作品',
    link: config.getBookmarkNewIllustsPageUrl(),
});

config.defaultNavs.push({
    name: '收藏的作品',
    link: config.getBookmarkIllustsPageUrl(),
});

config.defaultNavs.push({
    name: '排行榜',
    link: config.getRankPageUrl(),
});