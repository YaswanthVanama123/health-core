/* swap open/close side menu icons */
$('[data-toggle=collapse]').click(function () {
    $(this).find("i").toggleClass("fa-chevron-down fa-stack-1x fa-chevron-right fa-stack-1x");
});

$(function () {
    $('.subnav li a').filter(function () { return this.href == location.href }).parent().addClass('selected').siblings().removeClass('selected')
    $('.subnav li a').click(function () {
        $(this).parent().addClass('selected').siblings().removeClass('selected')
    })
})

// Scroll
var btn = $('#go-top-button');

$(window).scroll(function () {
    if ($(window).scrollTop() > 200) {
        btn.addClass('show');
    } else {
        btn.removeClass('show');
    }
});

btn.on('click', function (e) {
    e.preventDefault();
    $('html, body').animate({ scrollTop: 0 }, '300');
});

//Theme Selector
$("#select-theme").val(localStorage.getItem("theme"));

function SelectTheme(control) {
    try {
        var storTest = window['localStorage'];
        storTest.setItem("", ".");
        storTest.removeItem("");
    } catch (e) {
        console.log("Cannot save theme...")
    }

    var root = document.getElementsByTagName('html')[0];
    root.classList.remove(localStorage.getItem("theme"));
    localStorage.setItem("theme", control.value);
    root.classList.add(localStorage.getItem("theme"));
}

$(function () {
    $('.collapsable .btn').click(function (e) {
        e.stopPropagation();
    });
});

$(function () {
    $('.panel-heading .disableScroll').click(function (e) {
        e.stopPropagation();
    });
});

function RefreshPage() {
    window.location.reload();
}

// =========================
// Sidebar: Expanded ↔ Collapsed (icon-only) - Plain JS (no jQuery)
// ONLY FOR MAIN USER AREA - NOT ADMIN AREA
// =========================
(function () {
    "use strict";

    // Check if we're in the admin area - if so, skip this script
    var isAdminArea = window.location.pathname.toLowerCase().indexOf('/admin/') !== -1;
    if (isAdminArea) {
        console.log('Admin area detected - skipping sidebar collapse functionality');
        return;
    }

    var sidebarStorageKey = "gv.sidebar.state"; // "expanded" | "collapsed"

    function setIcon(isCollapsed) {
        var icon = document.querySelector('#sidebar-toggle i');
        if (!icon) {
            console.warn('Toggle icon not found!');
            return;
        }
        
        console.log('Setting icon - isCollapsed:', isCollapsed);
        console.log('Current icon classes:', icon.className);
        
        // Remove both classes first to ensure clean state
        icon.classList.remove('fa-angle-double-right', 'fa-angle-double-left');
        
        // When collapsed: show right arrow (→) to indicate "expand"
        // When expanded: show left arrow (←) to indicate "collapse"
        if (isCollapsed) {
            icon.classList.add('fa-angle-double-right');
            console.log('Added fa-angle-double-right (→)');
        } else {
            icon.classList.add('fa-angle-double-left');
            console.log('Added fa-angle-double-left (←)');
        }
        
        console.log('New icon classes:', icon.className);
    }

    function adjustLogo(isCollapsed) {
        var logoExpanded = document.querySelector('.gv-logo-expanded');
        var logoCollapsed = document.querySelector('.gv-logo-collapsed');
        
        if (logoExpanded && logoCollapsed) {
            if (isCollapsed) {
                logoExpanded.style.display = 'none';
                logoCollapsed.style.display = 'block';
            } else {
                logoExpanded.style.display = 'block';
                logoCollapsed.style.display = 'none';
            }
        }
    }

    function applyState(state) {
        var wrapper = document.getElementById('wrapper');
        if (!wrapper) return;

        var isCollapsed = state === 'collapsed';

        // Make sure sidebar is visible using legacy class
        wrapper.classList.add('toggled');
        wrapper.classList.toggle('sidebar-collapsed', isCollapsed);
        wrapper.classList.toggle('sidebar-expanded', !isCollapsed);

        // Update toggle icon
        setIcon(isCollapsed);

        // Adjust logo size
        adjustLogo(isCollapsed);

        console.log('Sidebar state applied:', state);
    }

    function getInitialState() {
        try {
            var saved = window.localStorage.getItem(sidebarStorageKey);
            return (saved === 'collapsed' || saved === 'expanded') ? saved : 'expanded';
        } catch (e) {
            return 'expanded';
        }
    }

    function toggleState() {
        var wrapper = document.getElementById('wrapper');
        if (!wrapper) {
            console.warn('Wrapper not found!');
            return;
        }

        var currentCollapsed = wrapper.classList.contains('sidebar-collapsed');
        var next = currentCollapsed ? 'expanded' : 'collapsed';
        
        console.log('=== TOGGLE CLICKED ===');
        console.log('Current state:', currentCollapsed ? 'collapsed' : 'expanded');
        console.log('Next state:', next);
        
        try {
            window.localStorage.setItem(sidebarStorageKey, next);
        } catch (e) {
            console.error('Failed to save state:', e);
        }

        applyState(next);
        console.log('=== TOGGLE COMPLETE ===');
    }

    document.addEventListener('DOMContentLoaded', function () {
        console.log('Sidebar script loaded for main user area');

        // Apply saved state on page load
        applyState(getInitialState());

        // Bind toggle button
        var toggleBtn = document.getElementById('sidebar-toggle');
        if (toggleBtn && !toggleBtn.__gvBound) {
            toggleBtn.__gvBound = true;
            toggleBtn.addEventListener('click', function (e) {
                e.preventDefault();
                toggleState();
            });
        }
    });
})();
