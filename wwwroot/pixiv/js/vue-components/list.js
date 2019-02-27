/*
[外部依赖]

config.js
[事件]
onprevpage(pageid)
onnextpage(pageid)
*/

Vue.component('list', {
    props: {
        imgs: {
            type: Array,
            required: true,
            default: [],
        },
        enablepageindicator: {
            type: Boolean,
            default: true,
        },
        hasprevpage: {
            type: Boolean,
            default: false,
        },
        hasnextpage: {
            type: Boolean,
            default: false,
        },
        pageid: {
            type: Number,
            default: 1,
        },
        visible: { 
            type: Boolean,
            default: true,
        },
    },
    template: '\
<div class="container" :style="{display:visible ? \'block\' : \'none\' }">\
    <div class="row">\
        <div v-for="item in imgs" class="col-xs-12 col-sm-6 col-md-4">\
            <profile v-if="item.user != undefined" :user="item.user" :enablefollowstate="false" :container="false" :bottommargin="false"></profile>\
            <div class="thumbnail">\
                <img class="img-responsive" style="width:100%;" :src="item.src" alt="">\
                <a class="btn btn-primary btn-block" :href="item.href" target="_blank">{{ item.title }}</a>\
            </div>\
        </div>\
    </div>\
    <div v-if="enablepageindicator" class="row">\
        <button class="btn btn-primary col-xs-6 col-sm-6 col-md-6" :disabled="!hasprevpage" @click="prevPage">上一页</button>\
        <button class="btn btn-primary col-xs-6 col-sm-6 col-md-6" :disabled="!hasnextpage" @click="nextPage">下一页</button>\
    </div>\
</div>',
    methods: {
        prevPage: function() {
            this.pageid--;
            this.$emit('onprevpage', this.pageid);
        },
        nextPage: function() {
            this.pageid++;
            this.$emit('onnextpage', this.pageid);
        },
        getPageId: function() {
            return this.pageid;
        },
        setPageId: function(pageId) {
            this.pageid = pageId;
        },
        reset: function() {
            this.pageid = 1;
        }
    },
    mounted: function() {
        this.$nextTick(function() {

        });
    },
})
