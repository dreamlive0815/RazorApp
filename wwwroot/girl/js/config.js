
var config = {
    apiUrlPrefix: '/girl/',
    pageUrlPrefix: './',
    defaultAutoSlideInterval: 5000,
    defaultHeadText: 'ZNGirls',
    defaultNavs: [
        { name: '切换模式', link: 'javascript:void(0)', onclick: function(index) { app.switchDisplayMode(); } },
    ],

    getProfileApiUrl: function(girlId) {
        return this.apiUrlPrefix + 'profile/' + girlId;
    },
    getAlbumsApiUrl: function(girlId) {
        return this.apiUrlPrefix + 'albumlist/' + girlId;
    },
    getAlbumInfoApiUrl: function(albumId) {
        return this.apiUrlPrefix + 'album/' + albumId;
    },
    getGalleryApiUrl: function(p) {
        return this.apiUrlPrefix + 'gallery/' + p;
    },
    getAlbumsPageUrl: function(girlId) {
        return this.pageUrlPrefix + 'albums.html?girlId=' + girlId;
    },
    getAlbumInfoPageUrl: function(albumId) {
        return this.pageUrlPrefix + 'albuminfo.html?albumId=' + albumId;
    },
    getGalleryPageUrl: function(p) {
        return this.pageUrlPrefix + 'gallery.html?p=' + p;
    }
};