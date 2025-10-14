<script>
    (function () {
        const navbar = document.querySelector(".navbar");
    if (!navbar) return;

    function onScroll() {
            if (window.scrollY > 50) {
        navbar.classList.add("scrolled");
            } else {
        navbar.classList.remove("scrolled");
            }
        }

    // run on load in case page loaded already scrolled
    window.addEventListener('load', onScroll);
    window.addEventListener('scroll', onScroll);
    })();
</script>
