/*
幻灯片播放图片
[外部依赖]
bootstrap
config.js
[事件]
onslidestart:过渡动画开始
onslideend:过渡动画结束
*/

Vue.component('carousel', {
    props: {
        id: {
            type: String,
            required: true,
            default: 'mycarousel',
        },
        imgs: {
            type: Object,
            required: true,
            default: [],
        },
        interval: { 
            type: Number,
            default: config.defaultAutoSlideInterval,
        },
        visible: {
            type: Boolean,
            default: false,
        },
        //下方指示器 快速定位到指定页
        enableindicator: {
            type: Boolean,
            default: false,
        },
    },
    template: '\
<div :style="{display:visible ? \'block\' : \'none\' }">\
    <div class="carousel slide" :id="id" @dblclick="dblclickHandler">\
        <ol v-if="enableindicator" class="carousel-indicators">\
            <li v-for="(img,index) in imgs" :data-target="\'#\' + id" :data-slide-to="index" :class="{active:index==0}"></li>\
        </ol>\
        <div class="carousel-inner">\
            <div v-for="(img,index) in imgs" :class="{item:true,active:index==0}">\
                <a :href="img.href ? img.href : \'javascript:void(0)\' " target="_blank"><img class="carousel-inner" :src="img.src"></a>\
                <div class="carousel-caption">{{ img.title }}[{{ index + 1 }}/{{ imgs.length }}]</div>\
            </div>\
        </div>\
        <a class="left carousel-control" :href="\'#\' + id" role="button" data-slide="prev">\
            <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>\
            <span class="sr-only">Previous</span>\
        </a>\
        <a class="right carousel-control" :href="\'#\' + id" role="button" data-slide="next">\
            <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>\
            <span class="sr-only">Next</span>\
        </a>\
        <button class="btn btn-primary btn-block" @click="switchAutoSilde">自动翻页({{ autoSlide ? \'开\' : \'关\' }})</button>\
    </div>\
</div>',
    data: function() {
        return {
            autoSlide: false
        }
    },
    methods: {
        dblclickHandler: function () {
            this.switchAutoSilde();
        },
        switchAutoSilde: function() {
            if(!this.autoSlide)
                this.autoSlideOn(config.defaultAutoSlideInterval);
            else
                this.autoSlideOff();
            this.autoSlide = !this.autoSlide;
        },
        autoSlideOn: function(interval) {
            var i = parseInt(this.interval);
            if(i != 0 && !isNaN(i)) interval = i;
            $('#' + this.id).carousel({
                interval: interval
            });
            console.log('autoSlideOn', interval);
        },
        autoSlideOff: function() {
            $('#' + this.id).carousel('pause');
            console.log('autoSlideOff');
        }
    },
    mounted: function() {
        this.$nextTick(function() {
            //初始化 强制关闭自动翻页
            this.autoSlideOff();
            var that = this;
            $('#' + this.id).on('slide.bs.carousel', function() {
                that.$emit('onslidestart');
            }).on('slid.bs.carousel', function() {
                that.$emit('onslideend');
            });
        });
    },
})
