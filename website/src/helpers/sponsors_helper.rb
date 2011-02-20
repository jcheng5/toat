require 'pp'
require 'fastimage'
module SponsorsHelper
  
  def begin_sponsors
  end
  
  def sponsor(name, url, logo)
    url = 'http://' + url unless url =~ /^http:\/\//
    width, height = FastImage.size("website/site/images/logos/#{logo}")
    partial('sponsor', :locals => { :name => name, 
                                    :url => url, 
                                    :logo => logo, 
                                    :width => width, 
                                    :height => height })
  end
  
  def end_sponsors
  end
  
end