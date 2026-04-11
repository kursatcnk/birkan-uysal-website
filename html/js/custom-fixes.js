// OZEL DUZELTMELER: YETENEKLER ICIN MOUSE ILE TUTUP SURUKLEME
(function(){
  function initDragScroll(track){
    if(!track){ return; }

    var isDown = false;
    var startX = 0;
    var startScrollLeft = 0;
    var moved = false;

    track.addEventListener('mousedown', function(e){
      isDown = true;
      moved = false;
      startX = e.pageX - track.offsetLeft;
      startScrollLeft = track.scrollLeft;
      track.classList.add('dragging');
      if(track.setPointerCapture && e.pointerId){
        try { track.setPointerCapture(e.pointerId); } catch(err) {}
      }
    });

    window.addEventListener('mousemove', function(e){
      if(!isDown){ return; }
      e.preventDefault();
      var x = e.pageX - track.offsetLeft;
      var walk = (x - startX) * 1.35;
      if(Math.abs(walk) > 4){ moved = true; }
      track.scrollLeft = startScrollLeft - walk;
    }, { passive: false });

    function stopDrag(){
      isDown = false;
      track.classList.remove('dragging');
    }

    window.addEventListener('mouseup', stopDrag);
    track.addEventListener('mouseleave', stopDrag);

    track.addEventListener('click', function(e){
      if(moved){
        e.preventDefault();
        e.stopPropagation();
      }
    }, true);

    track.addEventListener('wheel', function(e){
      if(Math.abs(e.deltaY) > Math.abs(e.deltaX)){
        e.preventDefault();
        track.scrollLeft += e.deltaY;
      }
    }, { passive: false });
  }

  function boot(){
    var track = document.querySelector('.skills_drag_track');
    if(!track){ return; }
    var computed = window.getComputedStyle(track);
    var isGrid = computed.display === 'grid';
    var hasOverflow = track.scrollWidth > track.clientWidth + 8;
    if(isGrid || !hasOverflow){ return; }
    initDragScroll(track);
  }

  if(document.readyState === 'loading'){
    document.addEventListener('DOMContentLoaded', boot);
  }else{
    boot();
  }
})();


// OZEL DUZELTMELER: PROJELER ALANINDA MODAL ACILAN KUTULAR
(function(){
  function bootProjectModals(){
    if(typeof jQuery === 'undefined'){ return; }

    var $ = jQuery;
    var modalBox = $('.edina_tm_modalbox');
    var closePopup = modalBox.find('.close');

    $('.project_modal_trigger').off('click.projectModal').on('click.projectModal', function(e){
      e.preventDefault();
      var details = $(this).siblings('.project_modal_content').html();
      if(!details){ return false; }
      $('body').addClass('modal');
      modalBox.addClass('opened');
      var wrap = modalBox.find('.description_wrap');
      wrap.html(details);
      wrap.scrollTop(0);
      return false;
    });

    closePopup.off('click.projectModal').on('click.projectModal', function(e){
      e.preventDefault();
      if(typeof edina_tm_close_modalbox === 'function'){
        edina_tm_close_modalbox();
      }else{
        modalBox.removeClass('opened');
        modalBox.find('.description_wrap').html('');
        $('body').removeClass('modal');
      }
      return false;
    });
  }

  if(document.readyState === 'loading'){
    document.addEventListener('DOMContentLoaded', bootProjectModals);
  }else{
    bootProjectModals();
  }
})();

