/*
幻灯片播放图片
[外部依赖]
bootstrap
config.js
[事件]
onprevpage(pageid)
onnextpage(pageid)
onslidestart:过渡动画开始
onslideend:过渡动画结束
*/

Vue.component('listandcarousel', {
    props: {
        //carousel
        carouselid: {
            type: String,
            required: true,
            default: 'mycarousel',
        },
        hasprevpage: {
            type: Boolean,
            default: false,
        },
        hasnextpage: {
            type: Boolean,
            default: false,
        },
        //carousel list
        imgs: {
            type: Object,
            required: true,
            default: [],
        },
        //carousel
        interval: { 
            type: Number,
            default: config.defaultAutoSlideInterval,
        },
        //list 对与carousel相反
        listvisible: {
            type: Boolean,
            default: true,
        },
        //list
        enablepageindicator: {
            type: Boolean,
            default: true,
        },
        //carousel下方指示器 快速定位到指定页
        enablecarouselindicator: {
            type: Boolean,
            default: false,
        },
    },
    template: '\
<div>\
    <list ref="list" :imgs="imgs" :hasprevpage="hasprevpage" :hasnextpage="hasnextpage" :visible="listvisible" :enablepageindicator="enablepageindicator" @onprevpage="prevPage" @onnextpage="nextPage"></list>\
    <carousel ref="carousel" :id="carouselid" :imgs="imgs" :interval="interval" :visible="!listvisible" :enableindicator="enablecarouselindicator" @onslidestart="slideStart" @onslideend="slideEnd"></carousel>\
</div>\
',
    methods: {
        getList: function() {
            return this.$refs.list;
        },
        getCarousel: function() {
            return this.$refs.carousel;
        },
        prevPage: function(pageId) {
            this.$emit('onprevpage', pageId);
        },
        nextPage: function(pageId) {
            this.$emit('onnextpage', pageId);
        },
        slideStart: function() {
            this.$emit('onslidestart');
        },
        slideEnd: function() {
            this.$emit('onslideend');
        },
        switchDisplayMode: function() {
            this.listvisible = !this.listvisible;
        },
        getPageId: function() {
            return this.getList().getPageId();
        },
        setPageId: function(pageId) {
            this.getList().setPageId(pageId);
        },
        resetList: function() {
            this.getList().reset();
        }
    },
    mounted: function() {
        this.$nextTick(function() {
            
        });
    },
})
