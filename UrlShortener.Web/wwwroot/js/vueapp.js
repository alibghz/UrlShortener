var urlShortener = new Vue({
    el: '#url-shortener',
    data: {
        createdUrl: "",
        errors: [],
        url: '',
        canSubmit: true
    },
    methods: {
        create: function () {
            let self = this;
            if (self.canSubmit !== true) return;
            self.errors = [];
            self.createdUrl = "";

            if (!self.url) {
                self.errors.push('URL is required');
                return;
            }

            if (!self.validUrl(self.url)) {
                self.errors.push('URL is not valid');
                return;
            }

            self.canSubmit = false;

            axios.post('/api/shortenedUrls/', { url: this.url })
                .then(function (response) {
                    console.log(response);
                    self.createdUrl = response.data;
                })
                .catch(function (error) {
                    self.errors.push(error);
                })
                .finally(function () {
                    self.canSubmit = true;
                });
        },
        validUrl: function (url) {
            return /(http|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/.test(url);
        }
    },
    created: function () {

    }
});