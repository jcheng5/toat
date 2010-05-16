#!/usr/bin/ruby

# https://secure.maestroweb.com/Login.aspx?OrgID=929
# https://secure.maestroweb.com/Details.aspx?OrgID=929&ItemID=1&selection=36

require 'rubygems'
require 'hpricot'
I_KNOW_I_AM_USING_AN_OLD_AND_BUGGY_VERSION_OF_LIBXML2 = true
require 'mechanize'
require 'pp'
require 'highline/import'
require 'builder'
#require 'uri'
#require 'net/http'
#require 'net/https'
require 'haml'
require 'optparse'

$agent = Mechanize.new

class NilClass
  def content()
    nil
  end
end

class Hash2Object
  def initialize(hash)
    @hash = hash
  end
  def method_missing(symbol, *args)
    return super(symbol, args) if args.length > 0
    @hash[symbol]
  end
end

def login(username, password)
  login_page = $agent.get('https://secure.maestroweb.com/Login.aspx?OrgID=929')
  login_form = login_page.form('LoginForm')
  login_form.txtUserField = username
  login_form.txtLogPassword = password
  $agent.submit(login_form, login_form.buttons.first)
end

def get_item(id)
  item = $agent.get("https://secure.maestroweb.com/Details.aspx?OrgID=929&ItemID=#{id}&selection=36")
  return nil unless item.search('#lblItemName').first
  
  item.search("#lblItemDesc br").each do |node|
    node.before("___BREAK___")
  end
  
  data = {
    :id => id,
    :name => item.search('#lblItemName').first.content.strip,
    :desc => item.search('#lblItemDesc').first.content.gsub(/___BREAK___/, "\n").strip,
    :value => item.search('#lblItemValue').first.content.strip,
    :donor => item.search('#lblItemDonors').first.content.strip,
    :tag => ''
  }
  
  $stderr.puts data[:name]
  
  #return nil unless (data[:name] =~ /^S - /)
  #data[:name].gsub!(/^S - /, '')
  if data[:name] =~ /^(.) - (.+)$/
   data[:tag] = $1
   data[:name] = $2
  end
  
  img_urls = []
  
  img_src = item.search('#imgItem').first['src']
  if img_src !~ /\bdefault\.gif$/
    img_remote_url = 'https://secure.maestroweb.com/' + img_src
    img_file = 'images/' + File.basename(img_remote_url)
    begin
      $agent.get(img_remote_url).save(img_file)
      img_urls.push 'file:///' + File.join(Dir.pwd, img_file)
    rescue
    end
  end
  
  item.parser.to_s.scan(/Pic\[\d\] = '([^']+)';/) do |match|
    img_remote_url = 'https://secure.maestroweb.com/' + $1
    img_file = 'images/' + File.basename(img_remote_url)
    begin
      img_path = 'file:///' + File.join(Dir.pwd, img_file)
      if !img_urls.member?(img_path)
        $agent.get(img_remote_url).save(img_file)
        img_urls.push(img_path)
        $stderr.puts(img_path)
      end
    rescue
    end
  end
  
  data[:img] = img_urls
  
  data
end

def download_url(url)
  if url =~ /([^\/]+)(\?|$)/
    filename = $1
    begin
      data = $agent.get_file(url)
    rescue
      return nil
    end
    File.open(filename, 'w') do |f|
      f.write data
    end
  end
end

password = gets #ask("Password: ") {|q| q.echo='*'}
login('Cheng', password)

xml = Builder::XmlMarkup.new(:target=>STDOUT, :indent=>0)
xml.Root do |root|
  200.times do |i|
    item = get_item(i)
    #$stderr.print(item ? '1' : '0')
    next unless item
    root.item do |b|
      item[:img].each do |img|
        b.image(:href => img)
      end
      puts
      b.tag(item[:tag])
      puts
      b.name(item[:name])
      puts
      b.desc(item[:desc])
      puts
      #b.value_label("Value: ")
      b.value(item[:value])
      puts
      #b.donor_label("Donated by: ")
      b.donor(item[:donor])
      puts
      #b.id("Auction lot #{item[:id]}")
      #puts
      b.id(item[:id])
      puts
    end
  end
end
