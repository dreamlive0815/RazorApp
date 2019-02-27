/*
[外部依赖]

[事件]
ontryfollow
ontryunfollow
*/

Vue.component('profile', {
    props: {
        user: {
            type: Object,
            default: {},
        },
        enablefollowstate: {
            type: Boolean,
            default: true,
        },
        follow: {
            type: Boolean,
            default: false,
        },
        visible: {
            type: Boolean,
            default: true,
        },
        container: {
            type: Boolean,
            default: true,
        },
        bottommargin: {
            type: Boolean,
            default: true,
        }
    },
    template: '\
<div :class="{container:container}" v-if="visible">\
    <div class="thumbnail" :style="style">\
        <img v-if="user.avatar != undefined" class="img-responsive" :src="user.avatar"></img>\
        <p class="centerp"><a :href="user.href" target="_blank">{{ user.name }}</a></p>\
        <button class="btn btn-block" v-if="enablefollowstate" :class="{\'btn-primary\':user.follow}" @click="switchFollowStateHandler">{{ user.follow ? \'关注中\' : \'未关注\' }}</button>\
    </div>\
</div>',
    computed: { 
        style() {
            if(this.bottommargin)  
                return {};
            else
                return {'margin-bottom':0};
        },
    },
    methods: {
        switchFollowStateHandler() {
            var user = this.user;
            if (user.follow)
                this.$emit('ontryunfollow', user.id, user.name);
            else
                this.$emit('ontryfollow', user.id, user.name);
        },
    },
    mounted: function() {
        this.$nextTick(function() {

        });
    },
})